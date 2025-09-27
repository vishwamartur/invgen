// Quotation Save and Output Actions
class QuotationSaveActions {
    constructor() {
        this.quotationId = null;
        this.isFormValid = false;
        this.initializeEventHandlers();
        this.updateButtonStates();
    }

    initializeEventHandlers() {
        // Handle form submission (not individual button clicks)
        $('#quotation-form').on('submit', (e) => {
            e.preventDefault();
            console.log('Form submitted via AJAX handler');
            this.saveQuotation();
        });

        // Save and download button
        $('#save-and-download-btn').on('click', (e) => {
            e.preventDefault();
            this.saveAndDownload();
        });

        // Print button
        $('#print-btn').on('click', (e) => {
            e.preventDefault();
            this.printQuotation();
        });

        // Form validation monitoring
        $('#quotation-form').on('input change', () => {
            this.validateForm();
        });

        // Line items change monitoring
        $(document).on('quotation:lineItemsChanged', () => {
            this.validateForm();
        });

        // Monitor line items table changes
        const observer = new MutationObserver(() => {
            this.validateForm();
        });

        const lineItemsBody = document.getElementById('line-items-body');
        if (lineItemsBody) {
            observer.observe(lineItemsBody, { childList: true });
        }
    }

    validateForm() {
        const form = document.getElementById('quotation-form');
        const customerId = $('#CustomerId').val();
        const lineItemsCount = $('#line-items-body tr.line-item-row').length;
        const quotationDate = $('#QuotationDate').val();

        // Basic validation - ProjectName is optional
        const hasCustomer = customerId && customerId !== '' && customerId !== '0';
        const hasLineItems = lineItemsCount > 0;
        const hasDate = quotationDate && quotationDate !== '';

        this.isFormValid = hasCustomer && hasLineItems && hasDate;

        console.log('Form validation:', {
            customerId: customerId,
            hasCustomer: hasCustomer,
            lineItemsCount: lineItemsCount,
            hasLineItems: hasLineItems,
            quotationDate: quotationDate,
            hasDate: hasDate,
            isValid: this.isFormValid
        });

        this.updateButtonStates();
        this.updateValidationFeedback(hasCustomer, hasLineItems, hasDate);
    }

    updateValidationFeedback(hasCustomer, hasLineItems, hasDate) {
        // Clear previous validation messages
        $('.validation-feedback').remove();

        if (!hasCustomer) {
            $('#CustomerId').addClass('is-invalid');
        } else {
            $('#CustomerId').removeClass('is-invalid');
        }

        if (!hasDate) {
            $('#QuotationDate').addClass('is-invalid');
        } else {
            $('#QuotationDate').removeClass('is-invalid');
        }

        if (!hasLineItems) {
            const emptyState = $('#empty-line-items');
            if (emptyState.length && !emptyState.hasClass('d-none')) {
                emptyState.addClass('border-warning');
            }
        } else {
            $('#empty-line-items').removeClass('border-warning');
        }
    }

    updateButtonStates() {
        const hasQuotationId = this.quotationId !== null;
        
        // Enable/disable buttons based on form validity and quotation state
        $('#save-btn').prop('disabled', !this.isFormValid);
        $('#save-and-download-btn').prop('disabled', !this.isFormValid);
        $('#print-btn').prop('disabled', !hasQuotationId);
    }

    async saveQuotation() {
        // Validate form first
        this.validateForm();

        if (!this.isFormValid) {
            this.showValidationErrors();
            return;
        }

        this.setButtonLoading('#save-btn', true);

        try {
            const formData = new FormData(document.getElementById('quotation-form'));

            console.log('Submitting quotation form...');
            const response = await fetch('/Quotations/Create', {
                method: 'POST',
                body: formData
            });

            console.log('Response received:', response.status, response.url);

            if (response.ok) {
                // Check if we got redirected (successful save)
                if (response.redirected) {
                    // Extract quotation ID from redirect URL
                    const redirectUrl = response.url;
                    const match = redirectUrl.match(/\/Details\/(\d+)/);
                    if (match) {
                        this.quotationId = parseInt(match[1]);
                        $('#quotation-id').val(this.quotationId);
                    }

                    this.showSuccess('Quotation saved successfully!');
                    this.updateButtonStates();

                    // Redirect to the details page
                    setTimeout(() => {
                        window.location.href = redirectUrl;
                    }, 1500);
                } else {
                    // Handle validation errors or other responses
                    const result = await response.text();
                    console.log('Server response:', result);
                    this.handleValidationErrors(result);
                }
            } else {
                const errorText = await response.text();
                console.error('Server error:', response.status, errorText);
                throw new Error(`Failed to save quotation: ${response.status} ${response.statusText}`);
            }
        } catch (error) {
            console.error('Save error:', error);
            this.showError('Failed to save quotation. Please try again. Check the console for details.');
        } finally {
            this.setButtonLoading('#save-btn', false);
        }
    }

    showValidationErrors() {
        const customerId = $('#CustomerId').val();
        const lineItemsCount = $('#line-items-body tr.line-item-row').length;
        const quotationDate = $('#QuotationDate').val();

        let errors = [];

        if (!customerId || customerId === '' || customerId === '0') {
            errors.push('Please select a customer');
        }

        if (!quotationDate || quotationDate === '') {
            errors.push('Please enter a quotation date');
        }

        if (lineItemsCount === 0) {
            errors.push('Please add at least one line item');
        }

        if (errors.length > 0) {
            this.showError('Please fix the following issues:\n• ' + errors.join('\n• '));
        }
    }

    async saveAndDownload() {
        if (!this.isFormValid) {
            this.showError('Please fill in all required fields and add at least one line item.');
            return;
        }

        this.setButtonLoading('#save-and-download-btn', true);

        try {
            // First save the quotation
            await this.saveQuotation();
            
            if (this.quotationId) {
                // Then download the PDF
                await this.downloadPdf();
            }
        } catch (error) {
            console.error('Save and download error:', error);
            this.showError('Failed to save and download quotation. Please try again.');
        } finally {
            this.setButtonLoading('#save-and-download-btn', false);
        }
    }

    async downloadPdf() {
        if (!this.quotationId) {
            this.showError('Please save the quotation first.');
            return;
        }

        try {
            const response = await fetch(`/Quotations/DownloadPdf/${this.quotationId}`);
            
            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `Quotation_${this.quotationId}.pdf`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
                
                this.showSuccess('PDF downloaded successfully!');
            } else {
                throw new Error('Failed to download PDF');
            }
        } catch (error) {
            console.error('Download error:', error);
            this.showError('Failed to download PDF. Please try again.');
        }
    }

    printQuotation() {
        if (!this.quotationId) {
            this.showError('Please save the quotation first.');
            return;
        }

        // Hide all no-print elements and show print-only elements
        $('.no-print').hide();
        $('.print-only').show();
        
        // Trigger browser print dialog
        window.print();
        
        // Restore visibility after print dialog closes
        setTimeout(() => {
            $('.no-print').show();
            $('.print-only').hide();
        }, 1000);
    }

    setButtonLoading(buttonSelector, isLoading) {
        const $button = $(buttonSelector);
        const $spinner = $button.find('.spinner-border');
        const $text = $button.find('.btn-text');
        
        if (isLoading) {
            $button.prop('disabled', true);
            $spinner.removeClass('d-none');
            $text.text($text.text().replace('Save', 'Saving'));
        } else {
            $button.prop('disabled', !this.isFormValid);
            $spinner.addClass('d-none');
            $text.text($text.text().replace('Saving', 'Save'));
        }
    }

    handleValidationErrors(responseText) {
        // Parse validation errors from response
        const $response = $(responseText);
        const errors = [];
        
        $response.find('.field-validation-error, .validation-summary-errors li').each(function() {
            const errorText = $(this).text().trim();
            if (errorText && !errors.includes(errorText)) {
                errors.push(errorText);
            }
        });
        
        if (errors.length > 0) {
            this.showError('Validation errors: ' + errors.join(', '));
        } else {
            this.showError('Please correct the form errors and try again.');
        }
    }

    showSuccess(message) {
        this.showToast(message, 'success');
    }

    showError(message) {
        this.showToast(message, 'error');
    }

    showToast(message, type) {
        // Create toast notification
        const toastId = 'toast-' + Date.now();
        const toastClass = type === 'success' ? 'bg-success' : 'bg-danger';
        
        const toastHtml = `
            <div id="${toastId}" class="toast ${toastClass} text-white" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="toast-header ${toastClass} text-white border-0">
                    <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>
                    <strong class="me-auto">${type === 'success' ? 'Success' : 'Error'}</strong>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;
        
        // Add to toast container or create one
        let $container = $('#toast-container');
        if ($container.length === 0) {
            $container = $('<div id="toast-container" class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 1055;"></div>');
            $('body').append($container);
        }
        
        $container.append(toastHtml);
        
        // Show toast
        const toast = new bootstrap.Toast(document.getElementById(toastId), {
            autohide: true,
            delay: type === 'success' ? 3000 : 5000
        });
        toast.show();
        
        // Remove from DOM after hiding
        $(`#${toastId}`).on('hidden.bs.toast', function() {
            $(this).remove();
        });
    }
}

// Initialize when document is ready
$(document).ready(function() {
    window.quotationSaveActions = new QuotationSaveActions();
});

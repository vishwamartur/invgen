/**
 * InvGen Professional - Quotation Creation JavaScript
 * Handles dynamic line items, calculations, and form interactions
 */

// Global quotation management object
window.QuotationCreate = window.QuotationCreate || {};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    QuotationCreate.init();
});

// Main initialization
QuotationCreate.init = function() {
    console.log('Initializing Quotation Create functionality...');
    
    // Initialize components
    QuotationCreate.initCustomerSelection();
    QuotationCreate.initLineItems();
    QuotationCreate.initCalculations();
    QuotationCreate.initFormValidation();
    QuotationCreate.initDateValidation();
    
    console.log('Quotation Create initialized successfully');
};

// Customer selection functionality
QuotationCreate.initCustomerSelection = function() {
    const customerSelect = document.getElementById('customer-select');
    const customerDetails = document.getElementById('customer-details');
    
    if (customerSelect) {
        customerSelect.addEventListener('change', function() {
            const customerId = this.value;
            
            if (customerId) {
                QuotationCreate.loadCustomerDetails(customerId);
            } else {
                customerDetails.classList.add('d-none');
            }
        });
    }
};

// Load customer details via AJAX
QuotationCreate.loadCustomerDetails = function(customerId) {
    const customerDetails = document.getElementById('customer-details');
    
    fetch(`/Quotations/GetCustomer/${customerId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const customer = data.customer;
                
                // Update customer display
                document.getElementById('customer-company').textContent = customer.companyName || customer.name;
                document.getElementById('customer-contact').textContent = 
                    `${customer.email || ''} ${customer.phone ? '• ' + customer.phone : ''}`.trim();
                document.getElementById('customer-address').textContent = 
                    `${customer.address || ''}, ${customer.city || ''} ${customer.postalCode || ''}`.trim();
                document.getElementById('customer-gst').textContent = 
                    customer.gstNumber ? `GST: ${customer.gstNumber}` : '';
                
                customerDetails.classList.remove('d-none');
            }
        })
        .catch(error => {
            console.error('Error loading customer details:', error);
        });
};

// Line items management
QuotationCreate.initLineItems = function() {
    let lineItemIndex = 0;

    // Add product line item buttons
    document.getElementById('add-line-item')?.addEventListener('click', function() {
        QuotationCreate.addLineItem('product');
    });

    document.getElementById('add-first-item')?.addEventListener('click', function() {
        QuotationCreate.addLineItem('product');
    });

    // Add custom line item buttons
    document.getElementById('add-custom-item')?.addEventListener('click', function() {
        QuotationCreate.addLineItem('custom');
    });

    document.getElementById('add-first-custom-item')?.addEventListener('click', function() {
        QuotationCreate.addLineItem('custom');
    });

    // Store line item index globally
    QuotationCreate.lineItemIndex = lineItemIndex;
};

// Add new line item
QuotationCreate.addLineItem = function(itemType = 'product') {
    const templateId = itemType === 'custom' ? 'custom-line-item-template' : 'line-item-template';
    const template = document.getElementById(templateId);
    const tbody = document.getElementById('line-items-body');
    const emptyState = document.getElementById('empty-line-items');

    if (!template || !tbody) return;

    // Clone template
    const clone = template.content.cloneNode(true);
    const row = clone.querySelector('.line-item-row');

    // Update indices and names
    const index = QuotationCreate.lineItemIndex++;
    QuotationCreate.updateLineItemIndices(clone, index);

    // Add event listeners
    QuotationCreate.attachLineItemEvents(clone, itemType);

    // Add to table
    tbody.appendChild(clone);

    // Hide empty state
    if (emptyState) {
        emptyState.classList.add('d-none');
    }

    // Update line numbers
    QuotationCreate.updateLineNumbers();

    // Focus on appropriate field
    if (itemType === 'custom') {
        const customNameInput = row.querySelector('.custom-item-name');
        if (customNameInput) {
            setTimeout(() => customNameInput.focus(), 100);
        }
    } else {
        const productSelect = row.querySelector('.product-select');
        if (productSelect) {
            setTimeout(() => productSelect.focus(), 100);
        }
    }
};

// Update line item indices
QuotationCreate.updateLineItemIndices = function(element, index) {
    const inputs = element.querySelectorAll('input, select');
    inputs.forEach(input => {
        const name = input.getAttribute('name');
        if (name) {
            input.setAttribute('name', name.replace(/\[\d+\]/, `[${index}]`));
        }
    });
};

// Attach events to line item
QuotationCreate.attachLineItemEvents = function(element, itemType = 'product') {
    // Product selection (for product items only)
    const productSelect = element.querySelector('.product-select');
    if (productSelect) {
        productSelect.addEventListener('change', function() {
            QuotationCreate.onProductChange(this);
        });
    }

    // Custom item name (for custom items only)
    const customItemName = element.querySelector('.custom-item-name');
    if (customItemName) {
        customItemName.addEventListener('input', function() {
            QuotationCreate.validateCustomItem(this.closest('.line-item-row'));
        });
    }

    // Unit selection (for custom items)
    const unitSelect = element.querySelector('.unit-input');
    if (unitSelect && itemType === 'custom') {
        unitSelect.addEventListener('change', function() {
            QuotationCreate.calculateLineTotal(this.closest('.line-item-row'));
        });
    }

    // Quantity and price changes
    const quantityInput = element.querySelector('.quantity-input');
    const priceInput = element.querySelector('.price-input');

    if (quantityInput) {
        quantityInput.addEventListener('input', function() {
            QuotationCreate.calculateLineTotal(this.closest('.line-item-row'));
        });
    }

    if (priceInput) {
        priceInput.addEventListener('input', function() {
            QuotationCreate.calculateLineTotal(this.closest('.line-item-row'));
        });
    }

    // Description textarea auto-resize (for custom items)
    const descriptionTextarea = element.querySelector('textarea.line-description');
    if (descriptionTextarea) {
        descriptionTextarea.addEventListener('input', function() {
            this.style.height = 'auto';
            this.style.height = this.scrollHeight + 'px';
        });
    }

    // Remove button
    const removeBtn = element.querySelector('.remove-line-item');
    if (removeBtn) {
        removeBtn.addEventListener('click', function() {
            QuotationCreate.removeLineItem(this.closest('.line-item-row'));
        });
    }
};

// Handle product selection change
QuotationCreate.onProductChange = function(selectElement) {
    const row = selectElement.closest('.line-item-row');
    const selectedOption = selectElement.selectedOptions[0];
    
    if (selectedOption && selectedOption.value) {
        const price = selectedOption.getAttribute('data-price');
        const unit = selectedOption.getAttribute('data-unit');
        const description = selectedOption.getAttribute('data-description');
        
        // Update fields
        const priceInput = row.querySelector('.price-input');
        const unitInput = row.querySelector('.unit-input');
        const descriptionInput = row.querySelector('.line-description');
        
        if (priceInput) priceInput.value = price || '0';
        if (unitInput) unitInput.value = unit || '';
        if (descriptionInput && !descriptionInput.value) {
            descriptionInput.value = description || '';
        }
        
        // Calculate line total
        QuotationCreate.calculateLineTotal(row);
    }
};

// Validate custom item
QuotationCreate.validateCustomItem = function(row) {
    const customItemName = row.querySelector('.custom-item-name');
    const priceInput = row.querySelector('.price-input');
    const unitSelect = row.querySelector('.unit-input');

    // Basic validation for required fields
    let isValid = true;

    if (customItemName && !customItemName.value.trim()) {
        customItemName.classList.add('is-invalid');
        isValid = false;
    } else if (customItemName) {
        customItemName.classList.remove('is-invalid');
        customItemName.classList.add('is-valid');
    }

    if (priceInput && (!priceInput.value || parseFloat(priceInput.value) <= 0)) {
        priceInput.classList.add('is-invalid');
        isValid = false;
    } else if (priceInput) {
        priceInput.classList.remove('is-invalid');
        priceInput.classList.add('is-valid');
    }

    if (unitSelect && !unitSelect.value) {
        unitSelect.classList.add('is-invalid');
        isValid = false;
    } else if (unitSelect) {
        unitSelect.classList.remove('is-invalid');
        unitSelect.classList.add('is-valid');
    }

    return isValid;
};

// Remove line item
QuotationCreate.removeLineItem = function(row) {
    const tbody = document.getElementById('line-items-body');
    const emptyState = document.getElementById('empty-line-items');
    
    row.remove();
    
    // Update line numbers
    QuotationCreate.updateLineNumbers();
    
    // Show empty state if no items
    if (tbody && tbody.children.length === 0 && emptyState) {
        emptyState.classList.remove('d-none');
    }
    
    // Recalculate totals
    QuotationCreate.calculateQuotationTotals();
};

// Update line numbers
QuotationCreate.updateLineNumbers = function() {
    const rows = document.querySelectorAll('.line-item-row');
    rows.forEach((row, index) => {
        const lineNumber = row.querySelector('.line-number');
        if (lineNumber) {
            lineNumber.textContent = index + 1;
        }
    });
};

// Calculate line total
QuotationCreate.calculateLineTotal = function(row) {
    const quantityInput = row.querySelector('.quantity-input');
    const priceInput = row.querySelector('.price-input');
    const lineTotalDisplay = row.querySelector('.line-total');
    const lineTotalInput = row.querySelector('.line-total-input');
    
    if (quantityInput && priceInput && lineTotalDisplay && lineTotalInput) {
        const quantity = parseFloat(quantityInput.value) || 0;
        const price = parseFloat(priceInput.value) || 0;
        const total = quantity * price;
        
        lineTotalDisplay.textContent = `₹${total.toFixed(2)}`;
        lineTotalInput.value = total.toFixed(2);
        
        // Recalculate quotation totals
        QuotationCreate.calculateQuotationTotals();
    }
};

// Initialize calculations
QuotationCreate.initCalculations = function() {
    // Tax rate change
    const taxRateInput = document.getElementById('tax-rate');
    if (taxRateInput) {
        taxRateInput.addEventListener('input', function() {
            document.getElementById('tax-rate-display').textContent = this.value;
            QuotationCreate.calculateQuotationTotals();
        });
    }
    
    // Tax inclusive toggle
    const taxInclusiveInput = document.getElementById('tax-inclusive');
    if (taxInclusiveInput) {
        taxInclusiveInput.addEventListener('change', function() {
            QuotationCreate.updateTaxInfo();
            QuotationCreate.calculateQuotationTotals();
        });
    }
    
    // Initial calculation
    QuotationCreate.updateTaxInfo();
    QuotationCreate.calculateQuotationTotals();
};

// Update tax information display
QuotationCreate.updateTaxInfo = function() {
    const taxInclusiveInput = document.getElementById('tax-inclusive');
    const taxInfoText = document.getElementById('tax-info-text');
    
    if (taxInclusiveInput && taxInfoText) {
        if (taxInclusiveInput.checked) {
            taxInfoText.textContent = 'Prices include tax';
        } else {
            taxInfoText.textContent = 'Tax will be calculated on line totals';
        }
    }
};

// Calculate quotation totals
QuotationCreate.calculateQuotationTotals = function() {
    const lineTotalInputs = document.querySelectorAll('.line-total-input');
    const taxRateInput = document.getElementById('tax-rate');
    const taxInclusiveInput = document.getElementById('tax-inclusive');
    
    let subtotal = 0;
    lineTotalInputs.forEach(input => {
        subtotal += parseFloat(input.value) || 0;
    });
    
    const taxRate = parseFloat(taxRateInput?.value) || 0;
    const isTaxInclusive = taxInclusiveInput?.checked || false;
    
    let taxAmount = 0;
    let finalSubtotal = subtotal;
    
    if (isTaxInclusive) {
        // Tax is included in the line totals
        finalSubtotal = subtotal / (1 + (taxRate / 100));
        taxAmount = subtotal - finalSubtotal;
    } else {
        // Tax is added to the line totals
        taxAmount = subtotal * (taxRate / 100);
    }
    
    const total = finalSubtotal + taxAmount;
    
    // Update displays
    document.getElementById('subtotal-display').textContent = `₹${finalSubtotal.toFixed(2)}`;
    document.getElementById('tax-display').textContent = `₹${taxAmount.toFixed(2)}`;
    document.getElementById('total-display').textContent = `₹${total.toFixed(2)}`;
    
    // Update hidden inputs
    document.getElementById('subtotal-input').value = finalSubtotal.toFixed(2);
    document.getElementById('tax-input').value = taxAmount.toFixed(2);
    document.getElementById('total-input').value = total.toFixed(2);
};

// Form validation - Disabled in favor of AJAX handling in quotation-save-actions.js
QuotationCreate.initFormValidation = function() {
    // Form validation is now handled by QuotationSaveActions class
    // This prevents conflicts between traditional form submission and AJAX
    console.log('Form validation delegated to QuotationSaveActions');
};

// Validate form
QuotationCreate.validateForm = function() {
    const customerSelect = document.getElementById('customer-select');
    const lineItems = document.querySelectorAll('.line-item-row');
    
    // Check customer selection
    if (!customerSelect?.value) {
        alert('Please select a customer.');
        customerSelect?.focus();
        return false;
    }
    
    // Check line items
    if (lineItems.length === 0) {
        alert('Please add at least one line item.');
        document.getElementById('add-line-item')?.click();
        return false;
    }
    
    // Validate each line item
    for (let row of lineItems) {
        const productSelect = row.querySelector('.product-select');
        const quantityInput = row.querySelector('.quantity-input');
        const priceInput = row.querySelector('.price-input');
        
        if (!productSelect?.value) {
            alert('Please select a product for all line items.');
            productSelect?.focus();
            return false;
        }
        
        if (!quantityInput?.value || parseFloat(quantityInput.value) <= 0) {
            alert('Please enter a valid quantity for all line items.');
            quantityInput?.focus();
            return false;
        }
        
        if (!priceInput?.value || parseFloat(priceInput.value) < 0) {
            alert('Please enter a valid price for all line items.');
            priceInput?.focus();
            return false;
        }
    }
    
    return true;
};

// Date validation
QuotationCreate.initDateValidation = function() {
    const quotationDate = document.querySelector('input[name="QuotationDate"]');
    const validUntil = document.querySelector('input[name="ValidUntil"]');
    
    if (quotationDate && validUntil) {
        quotationDate.addEventListener('change', function() {
            const quotationDateValue = new Date(this.value);
            const validUntilValue = new Date(validUntil.value);
            
            if (validUntilValue <= quotationDateValue) {
                const newValidUntil = new Date(quotationDateValue);
                newValidUntil.setDate(newValidUntil.getDate() + 30);
                validUntil.value = newValidUntil.toISOString().split('T')[0];
            }
        });
        
        validUntil.addEventListener('change', function() {
            const quotationDateValue = new Date(quotationDate.value);
            const validUntilValue = new Date(this.value);
            
            if (validUntilValue <= quotationDateValue) {
                alert('Valid until date must be after quotation date.');
                this.focus();
            }
        });
    }
};

// Show loading overlay
QuotationCreate.showLoading = function() {
    const loading = document.getElementById('quotation-loading');
    if (loading) {
        loading.classList.remove('d-none');
    }
};

// Hide loading overlay
QuotationCreate.hideLoading = function() {
    const loading = document.getElementById('quotation-loading');
    if (loading) {
        loading.classList.add('d-none');
    }
};

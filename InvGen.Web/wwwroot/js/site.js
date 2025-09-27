/**
 * InvGen Professional - Enhanced JavaScript Functions
 * Provides interactive features for the quotation generator
 */

// Global InvGen namespace
window.InvGen = window.InvGen || {};

// Initialize application when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded, initializing InvGen...');
    try {
        InvGen.init();
    } catch (error) {
        console.error('Error initializing InvGen:', error);
    }
});

// Main initialization function
InvGen.init = function() {
    // Initialize Bootstrap components
    InvGen.initBootstrapComponents();

    // Initialize form enhancements
    InvGen.initFormEnhancements();

    // Initialize search functionality
    InvGen.initSearchFunctionality();

    // Initialize loading states
    InvGen.initLoadingStates();

    // Initialize animations
    InvGen.initAnimations();

    console.log('InvGen Professional initialized successfully');
};

// Initialize Bootstrap components
InvGen.initBootstrapComponents = function() {
    console.log('Initializing Bootstrap components...');

    // Check if Bootstrap is loaded
    if (typeof bootstrap === 'undefined') {
        console.error('Bootstrap is not loaded!');
        return;
    }

    try {
        // Initialize tooltips
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });

        // Initialize popovers
        const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
        popoverTriggerList.map(function (popoverTriggerEl) {
            return new bootstrap.Popover(popoverTriggerEl);
        });

        console.log('Bootstrap components initialized successfully');
    } catch (error) {
        console.error('Error initializing Bootstrap components:', error);
    }

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            if (alert && alert.parentNode) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    });
};

// Initialize form enhancements
InvGen.initFormEnhancements = function() {
    // Add real-time validation
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', InvGen.handleFormSubmit);
    });

    // Format currency inputs
    const currencyInputs = document.querySelectorAll('input[data-currency]');
    currencyInputs.forEach(input => {
        input.addEventListener('blur', function() {
            InvGen.formatCurrency(this);
        });
        input.addEventListener('input', function() {
            InvGen.calculateTotals();
        });
    });

    // Format number inputs
    const numberInputs = document.querySelectorAll('input[type="number"]');
    numberInputs.forEach(input => {
        input.addEventListener('input', function() {
            InvGen.calculateTotals();
        });
    });

    // Add loading states to buttons
    const submitButtons = document.querySelectorAll('button[type="submit"]');
    submitButtons.forEach(button => {
        button.addEventListener('click', function() {
            InvGen.showButtonLoading(this);
        });
    });
};

// Initialize search functionality
InvGen.initSearchFunctionality = function() {
    const searchInputs = document.querySelectorAll('.search-input');
    searchInputs.forEach(input => {
        let searchTimeout;
        input.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                InvGen.performSearch(this);
            }, 300);
        });
    });
};

// Initialize loading states
InvGen.initLoadingStates = function() {
    // Show loading overlay for AJAX requests
    document.addEventListener('ajaxStart', function() {
        InvGen.showLoadingOverlay();
    });

    document.addEventListener('ajaxComplete', function() {
        InvGen.hideLoadingOverlay();
    });
};

// Initialize animations
InvGen.initAnimations = function() {
    // Add fade-in animation to cards
    const cards = document.querySelectorAll('.card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
        card.classList.add('fade-in');
    });

    // Add hover effects to interactive elements
    const interactiveElements = document.querySelectorAll('.btn, .card, .nav-link');
    interactiveElements.forEach(element => {
        element.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-2px)';
        });
        element.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });
};

// Format currency inputs
InvGen.formatCurrency = function(input) {
    let value = input.value.replace(/[^\d.]/g, '');
    if (value && !isNaN(value)) {
        input.value = parseFloat(value).toFixed(2);
    }
};

// Calculate line total
InvGen.calculateLineTotal = function(quantityInput, priceInput, totalElement) {
    const quantity = parseFloat(quantityInput.value) || 0;
    const price = parseFloat(priceInput.value) || 0;
    const total = quantity * price;
    if (totalElement) {
        totalElement.textContent = '₹' + total.toFixed(2);
    }
    return total;
};

// Product search functionality
function searchProducts(searchTerm, serviceType, callback) {
    const url = '/Products/SearchApi';
    const params = new URLSearchParams();
    if (searchTerm) params.append('term', searchTerm);
    if (serviceType) params.append('serviceType', serviceType);
    
    fetch(`${url}?${params}`)
        .then(response => response.json())
        .then(data => callback(data))
        .catch(error => console.error('Error searching products:', error));
}

// Customer search functionality
function searchCustomers(searchTerm, callback) {
    const url = '/Customers/SearchApi';
    const params = new URLSearchParams();
    if (searchTerm) params.append('term', searchTerm);
    
    fetch(`${url}?${params}`)
        .then(response => response.json())
        .then(data => callback(data))
        .catch(error => console.error('Error searching customers:', error));
}

// Quotation line item management
class QuotationLineItemManager {
    constructor(quotationId) {
        this.quotationId = quotationId;
        this.lineItems = [];
    }
    
    addLineItem(lineItem) {
        const url = '/Quotations/AddLineItem';
        return fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(lineItem)
        })
        .then(response => response.json());
    }
    
    updateLineItem(lineItem) {
        const url = '/Quotations/UpdateLineItem';
        return fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(lineItem)
        })
        .then(response => response.json());
    }
    
    deleteLineItem(lineItemId) {
        const url = `/Quotations/DeleteLineItem/${lineItemId}`;
        return fetch(url, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        })
        .then(response => response.json());
    }
}

// Tax calculation functions
function calculateSubTotal(lineItems) {
    return lineItems.reduce((sum, item) => sum + (item.quantity * item.unitPrice), 0);
}

function calculateTaxAmount(subTotal, taxRate, isTaxInclusive) {
    if (isTaxInclusive) {
        return (subTotal * taxRate) / (100 + taxRate);
    } else {
        return (subTotal * taxRate) / 100;
    }
}

function calculateTotalAmount(subTotal, taxAmount, isTaxInclusive) {
    if (isTaxInclusive) {
        return subTotal;
    } else {
        return subTotal + taxAmount;
    }
}

// Update quotation totals
function updateQuotationTotals() {
    const lineItemRows = document.querySelectorAll('.line-item-row');
    let subTotal = 0;
    
    lineItemRows.forEach(row => {
        const quantity = parseFloat(row.querySelector('.quantity-input').value) || 0;
        const unitPrice = parseFloat(row.querySelector('.unit-price-input').value) || 0;
        const lineTotal = quantity * unitPrice;
        
        row.querySelector('.line-total').textContent = '₹' + lineTotal.toFixed(2);
        subTotal += lineTotal;
    });
    
    const taxRate = parseFloat(document.getElementById('taxRate').value) || 0;
    const isTaxInclusive = document.getElementById('isTaxInclusive').checked;
    
    const taxAmount = calculateTaxAmount(subTotal, taxRate, isTaxInclusive);
    const totalAmount = calculateTotalAmount(subTotal, taxAmount, isTaxInclusive);
    
    document.getElementById('subTotal').textContent = '₹' + subTotal.toFixed(2);
    document.getElementById('taxAmount').textContent = '₹' + taxAmount.toFixed(2);
    document.getElementById('totalAmount').textContent = '₹' + totalAmount.toFixed(2);
}

// Confirmation dialogs
function confirmDelete(message) {
    return confirm(message || 'Are you sure you want to delete this item?');
}

// Form validation helpers
function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

function validatePhone(phone) {
    const re = /^[\+]?[1-9][\d]{0,15}$/;
    return re.test(phone.replace(/\s/g, ''));
}

// Enhanced UI functions
InvGen.showLoadingOverlay = function() {
    const overlay = document.getElementById('loading-overlay');
    if (overlay) {
        overlay.classList.remove('d-none');
    }
};

InvGen.hideLoadingOverlay = function() {
    const overlay = document.getElementById('loading-overlay');
    if (overlay) {
        overlay.classList.add('d-none');
    }
};

InvGen.showButtonLoading = function(button) {
    if (button) {
        button.classList.add('btn-loading');
        button.disabled = true;
        const originalText = button.innerHTML;
        button.setAttribute('data-original-text', originalText);
        button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';
    }
};

InvGen.hideButtonLoading = function(button) {
    if (button) {
        button.classList.remove('btn-loading');
        button.disabled = false;
        const originalText = button.getAttribute('data-original-text');
        if (originalText) {
            button.innerHTML = originalText;
        }
    }
};

// Enhanced form handling
InvGen.handleFormSubmit = function(event) {
    const form = event.target;
    const submitButton = form.querySelector('button[type="submit"]');

    // Show loading state
    if (submitButton) {
        InvGen.showButtonLoading(submitButton);
    }

    // Validate form
    if (!InvGen.validateForm(form)) {
        event.preventDefault();
        if (submitButton) {
            InvGen.hideButtonLoading(submitButton);
        }
        return false;
    }

    // Form is valid, allow submission
    return true;
};

InvGen.validateForm = function(form) {
    let isValid = true;
    const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');

    inputs.forEach(input => {
        if (!input.value.trim()) {
            InvGen.showFieldError(input, 'This field is required');
            isValid = false;
        } else {
            InvGen.clearFieldError(input);
        }

        // Validate email fields
        if (input.type === 'email' && input.value && !InvGen.validateEmail(input.value)) {
            InvGen.showFieldError(input, 'Please enter a valid email address');
            isValid = false;
        }

        // Validate phone fields
        if (input.type === 'tel' && input.value && !InvGen.validatePhone(input.value)) {
            InvGen.showFieldError(input, 'Please enter a valid phone number');
            isValid = false;
        }
    });

    return isValid;
};

InvGen.showFieldError = function(field, message) {
    field.classList.add('is-invalid');
    let feedback = field.parentNode.querySelector('.invalid-feedback');
    if (!feedback) {
        feedback = document.createElement('div');
        feedback.className = 'invalid-feedback';
        field.parentNode.appendChild(feedback);
    }
    feedback.textContent = message;
};

InvGen.clearFieldError = function(field) {
    field.classList.remove('is-invalid');
    const feedback = field.parentNode.querySelector('.invalid-feedback');
    if (feedback) {
        feedback.remove();
    }
};

// Enhanced search functionality
InvGen.performSearch = function(searchInput) {
    const searchTerm = searchInput.value.trim();
    const searchType = searchInput.getAttribute('data-search-type');

    if (searchTerm.length < 2) {
        return;
    }

    switch (searchType) {
        case 'products':
            InvGen.searchProducts(searchTerm, null, function(results) {
                InvGen.displaySearchResults(searchInput, results);
            });
            break;
        case 'customers':
            InvGen.searchCustomers(searchTerm, function(results) {
                InvGen.displaySearchResults(searchInput, results);
            });
            break;
    }
};

InvGen.displaySearchResults = function(searchInput, results) {
    let dropdown = searchInput.parentNode.querySelector('.search-dropdown');
    if (!dropdown) {
        dropdown = document.createElement('div');
        dropdown.className = 'search-dropdown position-absolute bg-white border rounded shadow-lg';
        dropdown.style.top = '100%';
        dropdown.style.left = '0';
        dropdown.style.right = '0';
        dropdown.style.zIndex = '1000';
        dropdown.style.maxHeight = '300px';
        dropdown.style.overflowY = 'auto';
        searchInput.parentNode.appendChild(dropdown);
    }

    dropdown.innerHTML = '';

    if (results.length === 0) {
        dropdown.innerHTML = '<div class="p-3 text-muted">No results found</div>';
    } else {
        results.forEach(result => {
            const item = document.createElement('div');
            item.className = 'p-2 border-bottom cursor-pointer';
            item.innerHTML = `
                <div class="fw-medium">${result.name}</div>
                <small class="text-muted">${result.description || result.email || ''}</small>
            `;
            item.addEventListener('click', function() {
                InvGen.selectSearchResult(searchInput, result);
                dropdown.remove();
            });
            dropdown.appendChild(item);
        });
    }

    // Hide dropdown when clicking outside
    document.addEventListener('click', function(event) {
        if (!searchInput.parentNode.contains(event.target)) {
            dropdown.remove();
        }
    }, { once: true });
};

InvGen.selectSearchResult = function(searchInput, result) {
    searchInput.value = result.name;

    // Trigger custom event for result selection
    const event = new CustomEvent('searchResultSelected', {
        detail: { result: result, input: searchInput }
    });
    searchInput.dispatchEvent(event);
};

// Enhanced notifications
InvGen.showNotification = function(message, type = 'info', duration = 5000) {
    const alertContainer = document.getElementById('alert-container');
    if (!alertContainer) return;

    const alertId = 'alert-' + Date.now();
    const alertHtml = `
        <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show shadow-lg" role="alert">
            <i class="fas fa-${InvGen.getIconForType(type)} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    alertContainer.insertAdjacentHTML('beforeend', alertHtml);

    // Auto-dismiss after duration
    setTimeout(() => {
        const alert = document.getElementById(alertId);
        if (alert) {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }
    }, duration);
};

InvGen.getIconForType = function(type) {
    const icons = {
        'success': 'check-circle',
        'danger': 'exclamation-triangle',
        'warning': 'exclamation-circle',
        'info': 'info-circle'
    };
    return icons[type] || 'info-circle';
};

// Calculate totals for quotations
InvGen.calculateTotals = function() {
    const lineItemRows = document.querySelectorAll('.line-item-row');
    if (lineItemRows.length === 0) return;

    let subTotal = 0;

    lineItemRows.forEach(row => {
        const quantity = parseFloat(row.querySelector('.quantity-input')?.value) || 0;
        const unitPrice = parseFloat(row.querySelector('.unit-price-input')?.value) || 0;
        const lineTotal = quantity * unitPrice;

        const lineTotalElement = row.querySelector('.line-total');
        if (lineTotalElement) {
            lineTotalElement.textContent = '₹' + lineTotal.toFixed(2);
        }
        subTotal += lineTotal;
    });

    const taxRateElement = document.getElementById('taxRate');
    const isTaxInclusiveElement = document.getElementById('isTaxInclusive');

    if (!taxRateElement) return;

    const taxRate = parseFloat(taxRateElement.value) || 0;
    const isTaxInclusive = isTaxInclusiveElement ? isTaxInclusiveElement.checked : false;

    const taxAmount = InvGen.calculateTaxAmount(subTotal, taxRate, isTaxInclusive);
    const totalAmount = InvGen.calculateTotalAmount(subTotal, taxAmount, isTaxInclusive);

    // Update display elements
    const subTotalElement = document.getElementById('subTotal');
    const taxAmountElement = document.getElementById('taxAmount');
    const totalAmountElement = document.getElementById('totalAmount');

    if (subTotalElement) subTotalElement.textContent = '₹' + subTotal.toFixed(2);
    if (taxAmountElement) taxAmountElement.textContent = '₹' + taxAmount.toFixed(2);
    if (totalAmountElement) totalAmountElement.textContent = '₹' + totalAmount.toFixed(2);
};

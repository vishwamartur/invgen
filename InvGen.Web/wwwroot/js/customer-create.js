/**
 * Customer Creation Form Enhancement Script
 * Provides real-time validation, formatting, and user experience improvements
 */

document.addEventListener('DOMContentLoaded', function() {
    initializeCustomerForm();
});

function initializeCustomerForm() {
    const form = document.getElementById('customerForm');
    const submitBtn = document.getElementById('submitBtn');
    
    if (!form || !submitBtn) return;

    // Initialize form validation
    setupFormValidation();
    
    // Initialize field formatting
    setupFieldFormatting();
    
    // Initialize form submission handling
    setupFormSubmission();
    
    // Initialize field focus management
    setupFocusManagement();
    
    console.log('Customer form initialized successfully');
}

function setupFormValidation() {
    const form = document.getElementById('customerForm');
    
    // Real-time validation for required fields
    const requiredFields = form.querySelectorAll('input[required], select[required]');
    requiredFields.forEach(field => {
        field.addEventListener('blur', () => validateField(field));
        field.addEventListener('input', () => clearFieldError(field));
    });
    
    // Email validation
    const emailField = document.querySelector('input[name="Email"]');
    if (emailField) {
        emailField.addEventListener('blur', () => validateEmail(emailField));
    }
    
    // Phone validation
    const phoneFields = document.querySelectorAll('input[type="tel"]');
    phoneFields.forEach(field => {
        field.addEventListener('blur', () => validatePhone(field));
    });
    
    // GST number validation
    const gstField = document.querySelector('input[name="GstNumber"]');
    if (gstField) {
        gstField.addEventListener('blur', () => validateGstNumber(gstField));
    }
    
    // Postal code validation
    const postalField = document.querySelector('input[name="PostalCode"]');
    if (postalField) {
        postalField.addEventListener('blur', () => validatePostalCode(postalField));
    }
}

function setupFieldFormatting() {
    // Auto-format phone numbers
    const phoneFields = document.querySelectorAll('input[type="tel"]');
    phoneFields.forEach(field => {
        field.addEventListener('input', (e) => formatPhoneNumber(e.target));
    });
    
    // Auto-format GST number
    const gstField = document.querySelector('input[name="GstNumber"]');
    if (gstField) {
        gstField.addEventListener('input', (e) => formatGstNumber(e.target));
    }
    
    // Auto-capitalize name fields
    const nameFields = document.querySelectorAll('input[name="Name"], input[name="CompanyName"], input[name="City"], input[name="State"]');
    nameFields.forEach(field => {
        field.addEventListener('blur', (e) => capitalizeField(e.target));
    });
    
    // Postal code formatting
    const postalField = document.querySelector('input[name="PostalCode"]');
    if (postalField) {
        postalField.addEventListener('input', (e) => formatPostalCode(e.target));
    }
}

function setupFormSubmission() {
    const form = document.getElementById('customerForm');
    const submitBtn = document.getElementById('submitBtn');
    
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        
        if (validateForm()) {
            showLoadingState(submitBtn);
            
            // Submit the form
            setTimeout(() => {
                form.submit();
            }, 500);
        } else {
            showValidationSummary();
        }
    });
}

function setupFocusManagement() {
    // Auto-focus first field
    const firstField = document.querySelector('input[name="Name"]');
    if (firstField) {
        firstField.focus();
    }
    
    // Enter key navigation
    const inputs = document.querySelectorAll('input, select, textarea');
    inputs.forEach((input, index) => {
        input.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                const nextInput = inputs[index + 1];
                if (nextInput) {
                    nextInput.focus();
                } else {
                    document.getElementById('submitBtn').focus();
                }
            }
        });
    });
}

// Validation Functions
function validateField(field) {
    const value = field.value.trim();
    const isRequired = field.hasAttribute('required');
    
    if (isRequired && !value) {
        showFieldError(field, `${getFieldLabel(field)} is required.`);
        return false;
    }
    
    clearFieldError(field);
    return true;
}

function validateEmail(field) {
    const email = field.value.trim();
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    
    if (email && !emailRegex.test(email)) {
        showFieldError(field, 'Please enter a valid email address.');
        return false;
    }
    
    clearFieldError(field);
    return true;
}

function validatePhone(field) {
    const phone = field.value.trim();
    const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
    
    if (phone && !phoneRegex.test(phone.replace(/[\s\-\(\)]/g, ''))) {
        showFieldError(field, 'Please enter a valid phone number.');
        return false;
    }
    
    clearFieldError(field);
    return true;
}

function validateGstNumber(field) {
    const gst = field.value.trim().toUpperCase();
    const gstRegex = /^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}[Z]{1}[0-9A-Z]{1}$/;
    
    if (gst && !gstRegex.test(gst)) {
        showFieldError(field, 'Please enter a valid GST number (e.g., 22AAAAA0000A1Z5).');
        return false;
    }
    
    clearFieldError(field);
    return true;
}

function validatePostalCode(field) {
    const postal = field.value.trim();
    const postalRegex = /^[0-9]{6}$/;
    
    if (postal && !postalRegex.test(postal)) {
        showFieldError(field, 'Please enter a valid 6-digit postal code.');
        return false;
    }
    
    clearFieldError(field);
    return true;
}

// Formatting Functions
function formatPhoneNumber(field) {
    let value = field.value.replace(/\D/g, '');
    
    if (value.length > 10) {
        value = value.substring(0, 10);
    }
    
    if (value.length >= 6) {
        value = value.replace(/(\d{5})(\d{5})/, '$1 $2');
    }
    
    field.value = value;
}

function formatGstNumber(field) {
    let value = field.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
    
    if (value.length > 15) {
        value = value.substring(0, 15);
    }
    
    field.value = value;
}

function formatPostalCode(field) {
    let value = field.value.replace(/\D/g, '');
    
    if (value.length > 6) {
        value = value.substring(0, 6);
    }
    
    field.value = value;
}

function capitalizeField(field) {
    if (field.value.trim()) {
        field.value = field.value.trim()
            .split(' ')
            .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
            .join(' ');
    }
}

// UI Helper Functions
function showFieldError(field, message) {
    field.classList.add('is-invalid');
    field.classList.remove('is-valid');
    
    const feedback = field.parentNode.querySelector('.invalid-feedback');
    if (feedback) {
        feedback.textContent = message;
    }
}

function clearFieldError(field) {
    field.classList.remove('is-invalid');
    if (field.value.trim()) {
        field.classList.add('is-valid');
    } else {
        field.classList.remove('is-valid');
    }
    
    const feedback = field.parentNode.querySelector('.invalid-feedback');
    if (feedback) {
        feedback.textContent = '';
    }
}

function getFieldLabel(field) {
    const label = document.querySelector(`label[for="${field.name}"]`) || 
                  document.querySelector(`label[for="${field.id}"]`);
    return label ? label.textContent.replace('*', '').trim() : field.name;
}

function validateForm() {
    const form = document.getElementById('customerForm');
    let isValid = true;
    
    // Validate all required fields
    const requiredFields = form.querySelectorAll('input[required], select[required]');
    requiredFields.forEach(field => {
        if (!validateField(field)) {
            isValid = false;
        }
    });
    
    // Validate email
    const emailField = form.querySelector('input[name="Email"]');
    if (emailField && !validateEmail(emailField)) {
        isValid = false;
    }
    
    // Validate phone fields
    const phoneFields = form.querySelectorAll('input[type="tel"]');
    phoneFields.forEach(field => {
        if (!validatePhone(field)) {
            isValid = false;
        }
    });
    
    // Validate GST number
    const gstField = form.querySelector('input[name="GstNumber"]');
    if (gstField && !validateGstNumber(gstField)) {
        isValid = false;
    }
    
    // Validate postal code
    const postalField = form.querySelector('input[name="PostalCode"]');
    if (postalField && !validatePostalCode(postalField)) {
        isValid = false;
    }
    
    return isValid;
}

function showLoadingState(button) {
    const btnText = button.querySelector('.btn-text');
    const spinner = button.querySelector('.spinner-border');
    
    button.disabled = true;
    btnText.textContent = 'Creating...';
    spinner.classList.remove('d-none');
}

function showValidationSummary() {
    const summary = document.getElementById('validationSummary');
    if (summary) {
        summary.classList.remove('d-none');
        summary.scrollIntoView({ behavior: 'smooth', block: 'center' });
        
        setTimeout(() => {
            summary.classList.add('d-none');
        }, 5000);
    }
}

// Export functions for testing
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        validateEmail,
        validatePhone,
        validateGstNumber,
        validatePostalCode,
        formatPhoneNumber,
        formatGstNumber,
        capitalizeField
    };
}

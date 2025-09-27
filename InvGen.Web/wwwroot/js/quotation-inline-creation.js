/**
 * Quotation Inline Creation Enhancement Script
 * Handles inline customer and product creation within quotation forms
 */

document.addEventListener('DOMContentLoaded', function() {
    initializeInlineCreation();
});

function initializeInlineCreation() {
    // Initialize customer creation
    setupCustomerCreation();
    
    // Initialize product creation
    setupProductCreation();
    
    // Initialize category creation
    setupCategoryCreation();
    
    console.log('Inline creation functionality initialized');
}

// Customer Creation Functions
function setupCustomerCreation() {
    const createCustomerBtn = document.getElementById('create-customer-btn');
    const customerModal = new bootstrap.Modal(document.getElementById('customerModal'));
    const saveCustomerBtn = document.getElementById('saveCustomerBtn');
    const customerForm = document.getElementById('customerForm');
    
    if (createCustomerBtn) {
        createCustomerBtn.addEventListener('click', () => {
            clearCustomerForm();
            customerModal.show();
        });
    }
    
    if (saveCustomerBtn) {
        saveCustomerBtn.addEventListener('click', () => {
            createCustomerInline();
        });
    }
    
    // Form validation on input
    if (customerForm) {
        const inputs = customerForm.querySelectorAll('input, textarea');
        inputs.forEach(input => {
            input.addEventListener('blur', () => validateCustomerField(input));
            input.addEventListener('input', () => clearFieldError(input));
        });
    }
}

function clearCustomerForm() {
    const form = document.getElementById('customerForm');
    form.reset();
    
    // Clear validation states
    const inputs = form.querySelectorAll('input, textarea');
    inputs.forEach(input => {
        input.classList.remove('is-valid', 'is-invalid');
        const feedback = input.parentNode.querySelector('.invalid-feedback');
        if (feedback) feedback.textContent = '';
    });
}

function validateCustomerField(field) {
    const value = field.value.trim();
    const isRequired = field.hasAttribute('required');
    
    if (isRequired && !value) {
        showFieldError(field, `${getFieldLabel(field)} is required.`);
        return false;
    }
    
    if (field.type === 'email' && value && !isValidEmail(value)) {
        showFieldError(field, 'Please enter a valid email address.');
        return false;
    }
    
    clearFieldError(field);
    return true;
}

function createCustomerInline() {
    const form = document.getElementById('customerForm');
    const saveBtn = document.getElementById('saveCustomerBtn');
    
    // Validate form
    let isValid = true;
    const inputs = form.querySelectorAll('input[required], textarea[required]');
    inputs.forEach(input => {
        if (!validateCustomerField(input)) {
            isValid = false;
        }
    });
    
    if (!isValid) {
        showToast('Please correct the validation errors.', 'error');
        return;
    }
    
    // Show loading state
    showLoadingState(saveBtn);
    
    // Collect form data
    const formData = new FormData(form);
    const customerData = Object.fromEntries(formData.entries());
    
    // Send AJAX request
    fetch('/Quotations/CreateCustomerInline', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(customerData)
    })
    .then(response => response.json())
    .then(data => {
        hideLoadingState(saveBtn);
        
        if (data.success) {
            // Add customer to dropdown
            addCustomerToDropdown(data.customer);
            
            // Select the new customer
            document.getElementById('customer-select').value = data.customer.id;
            
            // Trigger customer selection to load details
            document.getElementById('customer-select').dispatchEvent(new Event('change'));
            
            // Close modal
            bootstrap.Modal.getInstance(document.getElementById('customerModal')).hide();
            
            // Show success message
            showToast(data.message, 'success');
        } else {
            showToast(data.message, 'error');
        }
    })
    .catch(error => {
        hideLoadingState(saveBtn);
        showToast('An error occurred while creating the customer.', 'error');
        console.error('Error:', error);
    });
}

function addCustomerToDropdown(customer) {
    const select = document.getElementById('customer-select');
    const option = document.createElement('option');
    option.value = customer.id;
    option.textContent = customer.name;
    option.setAttribute('data-company', customer.companyName || '');
    option.setAttribute('data-email', customer.email || '');
    option.setAttribute('data-phone', customer.phone || '');
    option.setAttribute('data-address', customer.address || '');
    option.setAttribute('data-city', customer.city || '');
    option.setAttribute('data-state', customer.state || '');
    option.setAttribute('data-postal', customer.postalCode || '');
    
    select.appendChild(option);
}

// Product Creation Functions
function setupProductCreation() {
    // Handle create product buttons in line items
    document.addEventListener('click', function(e) {
        if (e.target.closest('.create-product-btn')) {
            e.preventDefault();
            const button = e.target.closest('.create-product-btn');
            const row = button.closest('.line-item-row');
            
            // Store reference to the current row
            window.currentProductRow = row;
            
            clearProductForm();
            const productModal = new bootstrap.Modal(document.getElementById('productModal'));
            productModal.show();
        }
    });
    
    const saveProductBtn = document.getElementById('saveProductBtn');
    if (saveProductBtn) {
        saveProductBtn.addEventListener('click', () => {
            createProductInline();
        });
    }
    
    // Form validation
    const productForm = document.getElementById('productForm');
    if (productForm) {
        const inputs = productForm.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            input.addEventListener('blur', () => validateProductField(input));
            input.addEventListener('input', () => clearFieldError(input));
        });
    }
}

function clearProductForm() {
    const form = document.getElementById('productForm');
    form.reset();
    
    // Clear validation states
    const inputs = form.querySelectorAll('input, select, textarea');
    inputs.forEach(input => {
        input.classList.remove('is-valid', 'is-invalid');
        const feedback = input.parentNode.querySelector('.invalid-feedback');
        if (feedback) feedback.textContent = '';
    });
}

function validateProductField(field) {
    const value = field.value.trim();
    const isRequired = field.hasAttribute('required');
    
    if (isRequired && !value) {
        showFieldError(field, `${getFieldLabel(field)} is required.`);
        return false;
    }
    
    if (field.type === 'number' && value && isNaN(parseFloat(value))) {
        showFieldError(field, 'Please enter a valid number.');
        return false;
    }
    
    clearFieldError(field);
    return true;
}

function createProductInline() {
    const form = document.getElementById('productForm');
    const saveBtn = document.getElementById('saveProductBtn');
    
    // Validate form
    let isValid = true;
    const inputs = form.querySelectorAll('input[required], select[required]');
    inputs.forEach(input => {
        if (!validateProductField(input)) {
            isValid = false;
        }
    });
    
    if (!isValid) {
        showToast('Please correct the validation errors.', 'error');
        return;
    }
    
    // Show loading state
    showLoadingState(saveBtn);
    
    // Collect form data
    const formData = new FormData(form);
    const productData = Object.fromEntries(formData.entries());
    
    // Convert checkbox value
    productData.isService = document.getElementById('productIsService').checked;
    
    // Send AJAX request
    fetch('/Quotations/CreateProductInline', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(productData)
    })
    .then(response => response.json())
    .then(data => {
        hideLoadingState(saveBtn);
        
        if (data.success) {
            // Add product to all product dropdowns
            addProductToDropdowns(data.product);
            
            // Select the new product in the current row
            if (window.currentProductRow) {
                const productSelect = window.currentProductRow.querySelector('.product-select');
                if (productSelect) {
                    productSelect.value = data.product.id;
                    productSelect.dispatchEvent(new Event('change'));
                }
            }
            
            // Close modal
            bootstrap.Modal.getInstance(document.getElementById('productModal')).hide();
            
            // Show success message
            showToast(data.message, 'success');
            
            // Clear reference
            window.currentProductRow = null;
        } else {
            showToast(data.message, 'error');
        }
    })
    .catch(error => {
        hideLoadingState(saveBtn);
        showToast('An error occurred while creating the product.', 'error');
        console.error('Error:', error);
    });
}

function addProductToDropdowns(product) {
    // Add to all existing product dropdowns
    const productSelects = document.querySelectorAll('.product-select');
    productSelects.forEach(select => {
        const option = document.createElement('option');
        option.value = product.id;
        option.textContent = `${product.name}${product.code ? ` (${product.code})` : ''}`;
        option.setAttribute('data-price', product.unitPrice || '0');
        option.setAttribute('data-unit', product.unit || '');
        option.setAttribute('data-description', product.description || '');
        
        select.appendChild(option);
    });
}

// Category Creation Functions
function setupCategoryCreation() {
    const createCategoryBtn = document.getElementById('create-category-btn');
    if (createCategoryBtn) {
        createCategoryBtn.addEventListener('click', () => {
            clearCategoryForm();
            const categoryModal = new bootstrap.Modal(document.getElementById('categoryModal'));
            categoryModal.show();
        });
    }
    
    const saveCategoryBtn = document.getElementById('saveCategoryBtn');
    if (saveCategoryBtn) {
        saveCategoryBtn.addEventListener('click', () => {
            createCategoryInline();
        });
    }
}

function clearCategoryForm() {
    const form = document.getElementById('categoryForm');
    form.reset();
    
    // Clear validation states
    const inputs = form.querySelectorAll('input, select, textarea');
    inputs.forEach(input => {
        input.classList.remove('is-valid', 'is-invalid');
        const feedback = input.parentNode.querySelector('.invalid-feedback');
        if (feedback) feedback.textContent = '';
    });
}

function createCategoryInline() {
    const form = document.getElementById('categoryForm');
    const saveBtn = document.getElementById('saveCategoryBtn');
    
    // Validate required fields
    const nameField = document.getElementById('categoryName');
    if (!nameField.value.trim()) {
        showFieldError(nameField, 'Category name is required.');
        showToast('Please enter a category name.', 'error');
        return;
    }
    
    // Show loading state
    showLoadingState(saveBtn);
    
    // Collect form data
    const formData = new FormData(form);
    const categoryData = Object.fromEntries(formData.entries());
    
    // Send AJAX request
    fetch('/Quotations/CreateCategoryInline', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(categoryData)
    })
    .then(response => response.json())
    .then(data => {
        hideLoadingState(saveBtn);
        
        if (data.success) {
            // Add category to dropdown
            addCategoryToDropdown(data.category);
            
            // Select the new category
            document.getElementById('productCategory').value = data.category.id;
            
            // Close modal
            bootstrap.Modal.getInstance(document.getElementById('categoryModal')).hide();
            
            // Show success message
            showToast(data.message, 'success');
        } else {
            showToast(data.message, 'error');
        }
    })
    .catch(error => {
        hideLoadingState(saveBtn);
        showToast('An error occurred while creating the category.', 'error');
        console.error('Error:', error);
    });
}

function addCategoryToDropdown(category) {
    const select = document.getElementById('productCategory');
    const option = document.createElement('option');
    option.value = category.id;
    option.textContent = category.name;
    
    select.appendChild(option);
}

// Utility Functions
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
    const label = document.querySelector(`label[for="${field.id}"]`);
    return label ? label.textContent.replace('*', '').trim() : field.name;
}

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function showLoadingState(button) {
    const btnText = button.querySelector('.btn-text');
    const spinner = button.querySelector('.spinner-border');
    
    button.disabled = true;
    if (btnText) btnText.textContent = 'Creating...';
    if (spinner) spinner.classList.remove('d-none');
}

function hideLoadingState(button) {
    const btnText = button.querySelector('.btn-text');
    const spinner = button.querySelector('.spinner-border');
    
    button.disabled = false;
    if (btnText) {
        if (button.id === 'saveCustomerBtn') btnText.textContent = 'Create Customer';
        else if (button.id === 'saveProductBtn') btnText.textContent = 'Create Product';
        else if (button.id === 'saveCategoryBtn') btnText.textContent = 'Create Category';
    }
    if (spinner) spinner.classList.add('d-none');
}

function showToast(message, type = 'info') {
    // Create toast element
    const toastContainer = document.querySelector('.toast-container') || createToastContainer();
    
    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'primary'} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas fa-${type === 'error' ? 'exclamation-triangle' : type === 'success' ? 'check-circle' : 'info-circle'} me-2"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { delay: 5000 });
    toast.show();
    
    // Remove toast element after it's hidden
    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
}

function createToastContainer() {
    const container = document.createElement('div');
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1055';
    document.body.appendChild(container);
    return container;
}

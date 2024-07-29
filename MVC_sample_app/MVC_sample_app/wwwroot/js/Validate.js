document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('addEmployeeForm');

    form.addEventListener('submit', function (event) {
        let isValid = true;

        // Date of Birth Validation
        const dobInput = document.getElementById('DateOfBirth');
        const dob = new Date(dobInput.value);
        const dobError = document.getElementById('dobError');
        const today = new Date();
        today.setHours(0, 0, 0, 0); // Set time to midnight
        if (dob > today) {
            dobError.style.display = 'block';
            isValid = false;
        } else {
            dobError.style.display = 'none';
        }

        // Email Validation
        const emailInput = document.getElementById('Email');
        const email = emailInput.value;
        const emailError = document.getElementById('emailError');
        const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
        if (!emailPattern.test(email)) {
            emailError.style.display = 'block';
            isValid = false;
        } else {
            emailError.style.display = 'none';
        }

        // File Validation
        const fileInput = document.getElementById('PictureFile');
        const fileError = document.getElementById('fileError');
        const allowedExtensions = /\.(jpg|jpeg|png)$/i;
        if (fileInput.files.length > 0 && !allowedExtensions.test(fileInput.value)) {
            fileError.style.display = 'block';
            isValid = false;
        } else {
            fileError.style.display = 'none';
        }

        if (!isValid) {
            event.preventDefault();
        }
    });
});

function showEditForm(email) {
    document.getElementById('editForm-' + email).style.display = 'table-row';
}

function hideEditForm(email) {
    document.getElementById('editForm-' + email).style.display = 'none';
}

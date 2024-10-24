// ==========index========

function redirectAfterDelay() {
    setTimeout(function () {
        window.location.href = "Login/Landing";
    },
        2000); // 2 seconds in milliseconds 
};

//===============OTP BOX================
document.addEventListener('DOMContentLoaded', (event) => {
    const otpInputs = document.querySelectorAll('.verification-inputs input');

    otpInputs.forEach((input, index) => {
        input.addEventListener('input', (event) => {
            const value = input.value;

            // Allow only integer input
            if (!/^[0-9]$/.test(value)) {
                input.value = ''; // Clear if not a number
            } else {
                // Automatically move to the next input after typing a digit
                if (input.value.length === 1 && index < otpInputs.length - 1) {
                    otpInputs[index + 1].focus();
                }
            }
        });

        input.addEventListener('keydown', (event) => {
            if (event.key === 'Backspace' && input.value === '' && index > 0) {
                otpInputs[index - 1].focus(); // Move back on backspace
            }
        });

        // Automatically focus on the first box when the page loads
        if (index === 0) {
            input.focus();
        }
    });
});
//=========================================
$(function () {  // Using the shorthand version, recommended
    // Validate form before submission
    $("#signupForm").on("submit", function (e) {
        var isValid = true;
        var fullName = $("#fullName").val();
        var phoneEmail = $("#phoneEmail").val();
        var password = $("#password").val();
        var confirmPassword = $("#confirmPassword").val();

        // Regex for 10-digit phone starting with 9 and for email validation
        var phoneRegex = /^9\d{9}$/;
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        // Check if full name is provided
        if (!fullName.trim()) {
            $("#fullName").addClass("is-invalid");
            isValid = false;
        } else {
            $("#fullName").removeClass("is-invalid");
        }

        // Check if phone or email is valid
        if (!(phoneRegex.test(phoneEmail) || emailRegex.test(phoneEmail))) {
            $("#phoneEmail").addClass("is-invalid");
            isValid = false;
        } else {
            $("#phoneEmail").removeClass("is-invalid");
        }

        // Check if password is provided and has at least 6 characters
        if (password.length < 6) {
            $("#password").addClass("is-invalid");
            isValid = false;
        } else {
            $("#password").removeClass("is-invalid");
        }

        // Check if confirm password matches
        if (confirmPassword !== password) {
            $("#confirmPassword").addClass("is-invalid");
            isValid = false;
        } else {
            $("#confirmPassword").removeClass("is-invalid");
        }

        // Prevent form submission if validation fails
        if (!isValid) {
            e.preventDefault();
        }
    });
});


//===============================

(function ($) {
    "use strict"; // Start of use strict

    // Tooltip
    $('[data-toggle="tooltip"]').tooltip();

    // Osahan Slider
    $('.osahan-slider').slick({
        infinite: true,
        autoplay: true,
        autoplaySpeed: 5000,
        centerMode: false,
        slidesToShow: 1,
        arrows: false,
        dots: true
    });

    // Sidebar Nav
    var $main_nav = $('#main-nav');
    var $toggle = $('.toggle');

    var defaultOptions = {
        disableAt: false,
        customToggle: $toggle,
        levelSpacing: 40,
        navTitle: '',
        levelTitles: true,
        levelTitleAsBack: true,
        pushContent: '#container',
        insertClose: 2
    };

    // call our plugin
    var Nav = $main_nav.hcOffcanvasNav(defaultOptions);

    // Select2
    $('.js-example-basic-single').select2();


})(jQuery); // End of use strict



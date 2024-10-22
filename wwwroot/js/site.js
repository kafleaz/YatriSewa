// Write your JavaScript code.

// Toggle sidebar visibility when clicking the menu button
document.getElementById('menu-toggle').addEventListener('click', function (event) {
    const sidebar = document.getElementById('sidebar');
    const isSidebarHidden = sidebar.classList.contains('hidden');

    // Toggle the sidebar's visibility
    if (isSidebarHidden) {
        sidebar.classList.remove('hidden'); // Show sidebar by removing 'hidden' class
    } else {
        sidebar.classList.add('hidden'); // Hide sidebar by adding 'hidden' class
    }

    event.stopPropagation(); // Prevent closing the sidebar immediately when the menu button is clicked
});

// Close sidebar if clicked outside of it
document.addEventListener('click', function (event) {
    const sidebar = document.getElementById('sidebar');
    const menuButton = document.getElementById('menu-toggle');

    // Check if the click is outside the sidebar and menu button
    const isClickInsideSidebar = sidebar.contains(event.target);
    const isClickMenuButton = menuButton.contains(event.target);

    // If click is outside both the sidebar and the menu button, and the sidebar is visible, hide it
    if (!isClickInsideSidebar && !isClickMenuButton && !sidebar.classList.contains('hidden')) {
        sidebar.classList.add('hidden'); // Add 'hidden' class to close the sidebar
    }
});


document.getElementById('search-toggle').addEventListener('click', function (event) {
    const searchBoxMobile = document.getElementById('search-box-mobile');
    const searchIcon = document.getElementById('search-toggle');

    // Toggle the sliding effect by adding/removing the 'search-active' class
    if (searchBoxMobile.classList.contains('d-none')) {
        searchIcon.classList.add('hide-search-icon');

        setTimeout(() => {
            searchBoxMobile.classList.remove('d-none');
            searchBoxMobile.classList.add('search-active');
        }), 5000; // Slight delay to allow transition effect
    } else {
        searchBoxMobile.classList.remove('search-active');
        setTimeout(() => {
            searchIcon.classList.remove('hide-search-icon'); // Show search icon
            searchBoxMobile.classList.add('d-none');
        }, 1000); // Wait for transition to finish before hiding search box
    }

    event.stopPropagation(); // Prevent immediate closing
});

// Close search box if clicked outside of it
document.addEventListener('click', function (event) {
    const searchBoxMobile = document.getElementById('search-box-mobile');
    const searchIcon = document.getElementById('search-toggle');
    const isClickInsideSearchBox = searchBoxMobile.contains(event.target);
    const isClickSearchButton = searchIcon.contains(event.target);

    if (!isClickInsideSearchBox && !isClickSearchButton && searchBoxMobile.classList.contains('search-active')) {
        searchBoxMobile.classList.remove('search-active');
        setTimeout(() => {
            searchIcon.classList.remove('hide-search-icon'); // Show search icon
            searchBoxMobile.classList.add('d-none');
        }, 1000); // Hide after transition
    }
});
//======================================Notification Dropdown======================================
document.getElementById('notificationDropdown').addEventListener('click', function (event) {
    event.preventDefault(); // Prevent default anchor behavior
    const notificationMenu = document.getElementById('notificationMenu');

    // Toggle the dropdown visibility
    if (notificationMenu.classList.contains('show-dropdown')) {
        notificationMenu.classList.remove('show-dropdown');
    } else {
        notificationMenu.classList.add('show-dropdown');
    }

    // Prevent click from closing immediately
    event.stopPropagation();
});

// Close the dropdown if clicking outside
document.addEventListener('click', function (event) {
    const notificationMenu = document.getElementById('notificationMenu');
    const notificationIcon = document.getElementById('notificationDropdown');

    if (!notificationIcon.contains(event.target) && !notificationMenu.contains(event.target)) {
        notificationMenu.classList.remove('show-dropdown');
    }
});

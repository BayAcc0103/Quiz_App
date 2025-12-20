// Theme utility functions for MAUI
window.themeUtils = {
    setTheme: function(theme) {
        // Ensure DOM is ready before making changes
        if (!document.documentElement || !document.body) {
            // If DOM is not ready, wait for it
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', function() {
                    window.themeUtils.setTheme(theme);
                });
                return;
            }
        }

        // Remove any existing theme classes
        document.documentElement.classList.remove('dark-theme', 'light-theme');
        document.body.classList.remove('dark-theme', 'light-theme');

        // Add the appropriate theme class
        if (theme === 'dark') {
            document.documentElement.classList.add('dark-theme');
            document.body.classList.add('dark-theme');
        } else {
            document.documentElement.classList.add('light-theme');
            document.body.classList.add('light-theme');
        }

        // Set background based on theme
        const backgroundImage = theme === 'dark'
            ? 'url(asset/bg-dark.jpg)'
            : 'url(asset/bg-light16.jpg)';

        // Only set background if the element exists
        if (document.body) {
            document.body.style.backgroundImage = backgroundImage;
            document.body.style.backgroundSize = 'cover';
            document.body.style.backgroundPosition = 'center';
            document.body.style.backgroundRepeat = 'no-repeat';
            document.body.style.backgroundAttachment = 'fixed';
        }
    }
};
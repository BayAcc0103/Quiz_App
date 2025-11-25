// Theme utility functions
window.themeUtils = {
    setTheme: function(theme) {
        document.documentElement.classList.toggle('dark-theme', theme === 'dark');
        document.documentElement.classList.toggle('light-theme', theme === 'light');
        
        // Set background based on theme
        const backgroundImage = theme === 'dark' 
            ? 'url(/asset/background/dark/bg-dark5.jpg)' 
            : 'url(/asset/background/light/bg-light5.jpg)';
            
        document.body.style.backgroundImage = backgroundImage;
        document.body.style.backgroundSize = 'cover';
        document.body.style.backgroundPosition = 'center';
        document.body.style.backgroundRepeat = 'no-repeat';
        document.body.style.backgroundAttachment = 'fixed';
    }
};
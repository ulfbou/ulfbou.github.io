console.log("app.js loaded and starting execution (Version: 2024-06-05_02).");
window.appJsFunctions = {
    highlightCode: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.querySelectorAll('pre code').forEach((block) => {
                hljs.highlightElement(block);
            });
        }
    },
    applyLazyLoading: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.querySelectorAll('img').forEach((img) => {
                img.setAttribute('loading', 'lazy');
            });
        }
    },
    getHeadings: function (elementId) {
        const element = document.getElementById(elementId);
        if (!element) return [];

        const headings = [];
        element.querySelectorAll('h2, h3').forEach((heading) => {
            if (heading.id) {
                headings.push({
                    id: heading.id,
                    text: heading.innerText,
                    level: parseInt(heading.tagName.substring(1))
                });
            }
        });
        return headings;
    },
    scrollToElement: function (id) {
        const element = document.getElementById(id);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }
};
window.localStorageHelper = {
    getItem: function (key) {
        return localStorage.getItem(key);
    },
    setItem: function (key, value) {
        localStorage.setItem(key, value);
    },
    removeItem: function (key) {
        localStorage.removeItem(key);
    }
};

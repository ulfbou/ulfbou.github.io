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
                    level: parseInt(heading.tagName.substring(1)) // h2 -> 2, h3 -> 3
                });
            }
        });
        return headings;
    },
    // Optional: Scroll to element
    scrollToElement: function (id) {
        const element = document.getElementById(id);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }
};

module.exports = {
    content: [
        './**/*.razor',       // Scannt Razor-Komponenten
        './**/*.html',        // Scannt HTML-Dateien
        './wwwroot/**/*.html' // Optional: Scannt Dateien im wwwroot
    ],
    theme: {
        extend: {}, // Platz für eigene Tailwind-Erweiterungen
    },
    plugins: [],
};

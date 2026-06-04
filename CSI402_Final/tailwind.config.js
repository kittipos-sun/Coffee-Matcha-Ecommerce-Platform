/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: ["class"],

  content: [
    "./Views/**/*.cshtml",   // 🔥 สำคัญมาก
    "./wwwroot/**/*.js"
  ],

  theme: {
    container: {
      center: true,
      padding: "2rem",
      screens: {
        "2xl": "1400px",
      },
    },

    extend: {
      fontFamily: {
        sans: ["Noto Sans JP", "system-ui", "sans-serif"],
        display: ["Zen Kaku Gothic New", "Noto Sans JP", "sans-serif"],
      },

      colors: {
        primary: "hsl(var(--primary))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",

        bamboo: {
          DEFAULT: "hsl(var(--bamboo))",
          light: "hsl(var(--bamboo-light))",
          dark: "hsl(var(--bamboo-dark))",
        },

        card: "hsl(var(--card))",
        muted: "hsl(var(--muted))",
        success: "hsl(var(--success))",
        warning: "hsl(var(--warning))",
      },

      borderRadius: {
        xl: "1rem",
        "2xl": "1.5rem",
      },

      boxShadow: {
        card: "0 4px 20px -4px rgba(0, 0, 0, 0.08)",
        elevated: "0 10px 40px -10px rgba(0, 0, 0, 0.12)",
      },

      keyframes: {
        "fade-in": {
          from: { opacity: "0" },
          to: { opacity: "1" },
        },
        "slide-up": {
          from: { opacity: "0", transform: "translateY(10px)" },
          to: { opacity: "1", transform: "translateY(0)" },
        },
      },

      animation: {
        "fade-in": "fade-in 0.5s ease-out",
        "slide-up": "slide-up 0.5s ease-out",
      },
    },
  },

  plugins: [require("tailwindcss-animate")],
};
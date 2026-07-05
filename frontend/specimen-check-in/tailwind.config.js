/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        navy: {
          50: '#f0f3f8',
          100: '#dce3ed',
          200: '#b8c7db',
          300: '#8fa5c3',
          400: '#6581a8',
          500: '#4a6490',
          600: '#3a4f75',
          700: '#2d3e5c',
          800: '#1e2d45',
          900: '#0f1b2d',
          950: '#091220',
        },
        teal: {
          50: '#f0fdfa',
          100: '#ccfbf1',
          200: '#99f6e4',
          300: '#5eead4',
          400: '#2dd4bf',
          500: '#14b8a6',
          600: '#0d9488',
          700: '#0f766e',
          800: '#115e59',
          900: '#134e4a',
        },
      },
    },
  },
  plugins: [],
}

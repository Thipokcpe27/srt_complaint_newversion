/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.cshtml',
    './Views/**/*.cshtml',
    './Pages/**/*.cs',
  ],
  theme: {
    extend: {
      colors: {
        'srt-navy': {
          DEFAULT: '#0D2E6E',
          light: '#1A4A9E',
          dark: '#081D47',
          50:  '#E8EEF8',
          100: '#C4D2EC',
          200: '#9DB4DE',
          300: '#7596D0',
          400: '#4E78C2',
          500: '#0D2E6E',
          600: '#0B2660',
          700: '#091E52',
          800: '#071644',
          900: '#050E36',
        },
        'srt-gold': {
          DEFAULT: '#C9A227',
          light: '#E4BE4A',
          dark: '#A07D0E',
        },
      },
      fontFamily: {
        'thai': ['Sarabun', 'sans-serif'],
      },
      fontSize: {
        'th-sm': ['0.9375rem', { lineHeight: '1.5rem' }],
        'th-base': ['1.0625rem', { lineHeight: '1.75rem' }],
        'th-lg': ['1.1875rem', { lineHeight: '1.875rem' }],
        'th-xl': ['1.375rem', { lineHeight: '2rem' }],
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
};

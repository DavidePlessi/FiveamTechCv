/**
 * plugins/vuetify.ts
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Styles
import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'

// Composables
import { createVuetify } from 'vuetify'

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  theme: {
    defaultTheme: 'cyber',
    themes: {
      cyber: {
        dark: true,
        colors: {
          background: '#050508', // sky-top
          surface: '#141419', // card-bg (approx opaque)
          primary: '#d10069', // accent-pink
          secondary: '#c46a00', // accent-orange
          error: '#CF6679',
          info: '#2196F3',
          success: '#4CAF50',
          warning: '#FB8C00',
        },
      },
    },
  },
})

// https://nuxt.com/docs/api/configuration/nuxt-config
import {API_URL} from "~/config";

export default defineNuxtConfig({
  compatibilityDate: '2024-04-03',
  devtools: { enabled: true },
  typescript: { typeCheck: true },
  modules: ['@nuxtjs/apollo'],
  apollo: {
    clients: {
      default: {
        httpEndpoint: `${API_URL}/graphql`
      }
    },
  },
})

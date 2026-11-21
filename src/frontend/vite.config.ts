import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    // In development, forward API calls to the backend (https profile in launchSettings.json).
    // `secure: false` accepts the ASP.NET Core development certificate.
    proxy: {
      '/api': {
        target: 'https://localhost:7134',
        secure: false,
      },
    },
  },
})

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath } from 'url';
import { resolve } from 'path';

export default defineConfig(({ mode }) => {
  const apiTarget = process.env.VITE_API_PROXY_TARGET || 'http://localhost:5000';
  const rootPath = fileURLToPath(new URL('.', import.meta.url));
  const srcPath = resolve(rootPath, 'src');

  return {
    plugins: [vue()],
    resolve: {
      alias: { '@': srcPath },
    },
    server: {
      proxy: {
        '/api': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  }
})

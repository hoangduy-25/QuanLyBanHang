import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const apiTarget = env.VITE_API_TARGET || 'https://localhost:44329';

  return {
    base: './',
    plugins: [react()],
    optimizeDeps: {
      include: [
        'react',
        'react-dom',
        'react-dom/client',
        'react/jsx-dev-runtime',
        'react-router-dom',
        'lucide-react',
      ],
    },
    server: {
      port: 5173,
      proxy: {
        '/api': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
        },
        '/images': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
        },
      },
      warmup: {
        clientFiles: [
          './src/main.tsx',
          './src/App.tsx',
          './src/context/AppContext.tsx',
          './src/components/layout/StorefrontLayout.tsx',
          './src/pages/storefront/CatalogPage.tsx',
        ],
      },
    },
  };
});

import { fileURLToPath } from 'node:url'
import { mergeConfig, defineConfig } from 'vitest/config'
import viteConfig from './vite.config.ts'

// Frontend unit tests live in tests/frontend at the repository root, outside this project.
const testsDir = fileURLToPath(new URL('../../tests/frontend', import.meta.url))

export default mergeConfig(
  viteConfig,
  defineConfig({
    // The tests directory has no node_modules of its own; resolve these packages from this project.
    resolve: {
      dedupe: ['vue', 'vitest', '@vue/test-utils'],
    },
    server: {
      fs: { allow: ['.', testsDir] },
    },
    test: {
      environment: 'jsdom',
      root: fileURLToPath(new URL('./', import.meta.url)),
      dir: testsDir,
      include: ['**/*.spec.ts'],
    },
  }),
)

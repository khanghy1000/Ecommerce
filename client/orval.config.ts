import { defineConfig } from 'orval';

export default defineConfig({
  petstore: {
    output: {
      target: 'src/lib/models',
      schemas: 'src/lib/models',
      baseUrl: 'http://localhost:5213/scalar',
      mock: false,
      prettier: true,
    },
    input: {
      target: 'http://localhost:5213/openapi/v1.json',
    },
  },
});

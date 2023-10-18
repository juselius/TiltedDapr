import { defineConfig } from 'vite'
import { resolve } from 'path'

var clientPort = process.env.CLIENT_PORT == null ? 8080 : parseInt(process.env.CLIENT_PORT);
var serverPort = process.env.SERVER_PORT == null ? 8085 : parseInt(process.env.SERVER_PORT);
serverPort = process.env.SERVER_PROXY_PORT == null ? serverPort : parseInt(process.env.SERVER_PROXY_PORT);

var proxy = {
  target: `http://127.0.0.1:${serverPort}/`,
  changeOrigin: false,
  secure: false,
  ws: true
}

export default defineConfig({
    build: {
        rollupOptions: {
            input: {
                main: resolve(__dirname, './src/Client/index.html'),
            },
        },
    },
    // config options
    server: {
        port: clientPort,
        host: '0.0.0.0',
        proxy: {
            '/api': proxy
        }
    }
})
# === ЕТАП 1: Збірка (Node.js) ===
FROM node:20-alpine AS build
WORKDIR /app

# Копіюємо конфіги і ставимо залежності
COPY package*.json ./
RUN npm install

# Копіюємо весь код і білдимо проект
COPY . .
RUN npm run build --configuration=production

# === ЕТАП 2: Роздача (Nginx) ===
FROM nginx:alpine

# Копіюємо збірку Angular
COPY --from=build /app/dist/app-fe /usr/share/nginx/html

# ДОДАЄМО НАШ НОВИЙ КОНФІГ:
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

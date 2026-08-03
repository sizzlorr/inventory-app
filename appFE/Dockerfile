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

# Копіюємо зібрані файли з першого етапу в папку, з якої Nginx роздає статику
# УВАГА: шлях /dist/inventory-frontend/browser актуальний для Angular 17+
COPY --from=build /app/dist/app-fe /usr/share/nginx/html

# Відкриваємо 80 порт
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]

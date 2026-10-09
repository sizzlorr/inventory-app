# Setup Instructions for Local Development

## Before running the application, follow these steps:

### 1. Create `.env` file from template
```bash
cp .env.example .env
```

### 2. Edit `.env` with your own values
Replace the placeholder values in `.env` with secure credentials:
- Generate a strong JWT_SECRET_KEY (minimum 32 characters)
- Set secure database password
- Set secure admin password

Example:
```bash
DB_USER=myuser
DB_PASSWORD=your_secure_db_password_here
DB_NAME=inventory_db
JWT_SECRET_KEY=your_32_character_minimum_secret_key_here
ADMIN_USERNAME=admin
ADMIN_PASSWORD=your_secure_admin_password_here
```

### 3. Create `docker-compose.yml` from template
```bash
cp docker-compose.example.yml docker-compose.yml
```

### 4. Run Docker containers
```bash
docker-compose up -d
```

## Important Security Notes

- **Never commit `.env` file** - It's already in `.gitignore`
- **Never commit `docker-compose.yml`** - It's already in `.gitignore`
- Keep `.env.example` updated as a reference
- Use strong, random passwords for production
- Rotate JWT_SECRET_KEY regularly for production

## File Structure

- `.env.example` - Template for environment variables (safe to commit)
- `.env` - Your local environment variables (NEVER commit this)
- `docker-compose.example.yml` - Template for Docker setup (safe to commit)
- `docker-compose.yml` - Your local Docker configuration (NEVER commit this)


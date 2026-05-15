# Портал недвижимости — RieltorCRM

Веб-приложение публичного портала недвижимости. Незарегистрированные пользователи просматривают объекты, клиенты бронируют и покупают, агенты ведут листинги, администратор модерирует, OfficeManager управляет компанией, Accountant — статистика и экспорт.

> Курсовая работа. Геращенко Кирилл С.

---

## Технологии

| Слой | Стек |
|------|------|
| Backend | .NET 8 / ASP.NET Core Web API |
| ORM | Entity Framework Core 8 + SQLite |
| Авторизация | ASP.NET Core Identity + JWT Bearer |
| Frontend | React 18 + TypeScript + Vite |
| HTTP | Axios + React Router v6 |

---

## Структура проекта

```
RieltorCRM-main/
├── docker-compose.yml
├── backend/
│   ├── Controllers/          # API-контроллеры (7 штук)
│   ├── Models/               # Сущности EF Core
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Data/                 # DbContext + SeedData
│   ├── Migrations/           # EF Core миграции
│   ├── Properties/           # launchSettings.json
│   ├── wwwroot/uploads/      # Загруженные фото (runtime)
│   ├── Program.cs
│   ├── appsettings.json
│   ├── RieltorCRM.csproj
│   └── Dockerfile
└── frontend/
    ├── src/
    │   ├── api/              # Axios-клиенты по ролям
    │   ├── pages/            # Страницы по ролям
    │   └── components/       # Navbar и общие компоненты
    ├── nginx.conf            # Nginx конфиг для Docker
    ├── vite.config.ts
    ├── package.json
    └── Dockerfile
```

---

## Запуск через Docker

### Требования
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Запуск

```bash
docker-compose up --build
```

| Сервис | Адрес |
|--------|-------|
| Сайт (Nginx + React) | http://localhost |
| API / Swagger | http://localhost:8080/swagger |

При первом запуске база данных создаётся автоматически, все тестовые аккаунты и демо-объекты заполняются без каких-либо дополнительных действий.

Данные сохраняются в Docker volumes (`db_data` — база SQLite, `uploads_data` — фото объектов) и не теряются при перезапуске контейнеров.

```bash
# Остановить
docker-compose down

# Сбросить все данные (удалить volumes)
docker-compose down -v
```

---

## Локальный запуск (без Docker)

### Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- EF Core Tools: `dotnet tool install --global dotnet-ef`

### 1. Backend

```bash
cd backend

dotnet restore
dotnet run
```

Backend запускается на **http://localhost:5078**.  
Swagger UI: **http://localhost:5078/swagger**

При первом запуске автоматически:
- Применяются миграции (`rieltor.db` создаётся в `backend/`)
- Создаётся компания "Риелтор Плюс"
- Создаются тестовые пользователи всех ролей
- Добавляются 5 демо-объектов недвижимости

### 2. Frontend (отдельный терминал)

```bash
cd frontend

npm install
npm run dev
```

Frontend запускается на **http://localhost:5173**.  
Запросы `/api/*` и `/uploads/*` автоматически проксируются на `http://localhost:5078`.

---

## Тестовые аккаунты

| Роль | Email | Пароль |
|------|-------|--------|
| Admin | admin@rioter.ru | Admin123! |
| Agent | agent@rioter.ru | Agent123! |
| OfficeManager | manager@rioter.ru | Manager123! |
| Accountant | buh@rioter.ru | Buh123! |
| Client | client@rioter.ru | Client123! |

Зарегистрировать нового клиента: `/register` на сайте.

---

## Роли и возможности

| Роль | Возможности |
|------|-------------|
| **Аноним** | Просмотр объектов, фильтрация по типу/городу/цене |
| **Client** | Бронирование и покупка объектов, история сделок, отмена |
| **Agent** | Создание и редактирование листингов, загрузка/удаление фото, дашборд |
| **OfficeManager** | Управление сотрудниками компании, клиентская база, статистика |
| **Accountant** | Статистика сделок компании, экспорт в CSV |
| **Admin** | Модерация объектов, управление всеми пользователями и компаниями |

---

## API (краткий обзор)

| Группа | Базовый путь | Авторизация |
|--------|-------------|-------------|
| Публичный | `/api/public/properties` | нет |
| Авторизация | `/api/auth` | нет / JWT |
| Клиент | `/api/client/bookings` | Client |
| Агент | `/api/agent/properties` | Agent |
| Администратор | `/api/admin` | Admin |
| ОфисМенеджер | `/api/manager` | OfficeManager |
| Бухгалтер | `/api/accountant` | Accountant |

Полный список эндпоинтов — в [РАЗБОР_ПРОЕКТА.md](РАЗБОР_ПРОЕКТА.md) или через Swagger UI.

---

## Процесс модерации объектов

1. Агент создаёт объект → статус **PendingApproval**
2. Администратор видит очередь в разделе "Модерация"
3. Одобрение → статус **Available** (объект виден на портале)
4. Отклонение → статус **ApprovalRejected** + причина агенту
5. При редактировании объект снова уходит на модерацию

---

## Сброс данных

```bash
# Удалить БД — при следующем dotnet run всё пересоздастся:
rm backend/rieltor.db
cd backend && dotnet run

# Пересоздать миграции (после изменения моделей):
cd backend
rm -rf Migrations/
dotnet ef migrations add InitialPortal
dotnet run
```

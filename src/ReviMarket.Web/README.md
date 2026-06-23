# ReviMarket.Web — ASP.NET Core MVC

Backend-версия ReviMarket.

## Что уже есть

- ASP.NET Core MVC
- ASP.NET Identity
- Регистрация
- Вход / выход
- Роли: `Customer`, `Creator`, `Admin`
- EF Core + SQLite
- Каталог услуг и товаров
- Биржа заказов
- Исполнители
- Профиль
- Заготовка сообщений

## Как запустить локально

```bash
cd src/ReviMarket.Web
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

После запуска открыть адрес из консоли, обычно:

```text
https://localhost:5001
```

## Регистрация

На сайте есть страница:

```text
/Account/Register
```

Типы аккаунта:

- Заказчик — может создавать заказы.
- Исполнитель — может добавлять товары / услуги.

## Важно

Сейчас это первая backend-основа. Дальше нужно добавить:

- миграцию в репозиторий или автогенерацию базы;
- настоящие отклики на заказы;
- полноценные чаты;
- админ-панель;
- модерацию;
- безопасные сделки;
- платежи через тестовый режим провайдера.

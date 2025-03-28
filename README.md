# AdPlatformStorage

AdPlatformStorage – сервис поиска рекламных площадок по регионам с использованием Trie-дерева.

# 🔍 Описание

Сервис предоставляет API для:

- Загрузки данных о рекламных площадках из текстовых файлов
- Мгновенного поиска площадок по регионам
- Оптимизированного хранения данных в памяти

## Функциональность

- Хранилище организовано через структуру Trie-дерева ( Bor )
- Поддерживает In-memory для быстрого доступа к данным
- Может быть далеко не оптимально для огромных данных размером 10 000 000 - нужно добавлять параллелизм

# 📌 Примеры API-запросов

Сервис тестировался через Postman

## 1. Загрузка данных (POST)

### Endpoint:

```POST /api/query/upload```

Принимает .txt файл формата:

```
AdPlatformName:/region1/path,/region2/path
ExamplePlatform:/ru/moscow,/us/new-york
```

## 2. Поиск площадок (GET)

### Endpoint:

```GET /api/query/platforms```

Тело запроса:

```http
Content-Type: application/json

{
  "source": "/ru/moscow"
}
```

Пример ответа:

```JSON
{
    "name": "/us/new-york",
    "sources": [
        "ExamplePlatform"
    ]
}
```

## 3. Формат ошибок

```JSON
{
    "exceptionType": "InvalidFormatFileErrorException",
    "message": "Invalid file format",
    "details": {
        "errorContextType": "WriteInStorageAsync",
        "innerException": null,
        "messageException": "Relative paths must start with '/': 'фывыфвф/us/new-york'",
        "validationExample": "Valid format examples: AdPlatform:/ru/moscow ; Campaign_123:/ny/summer-sale,/la/fall-offer ; Test:/lon/price?id=123"
    }
}
```





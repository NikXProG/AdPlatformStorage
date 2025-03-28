# AdPlatformStorage

AdPlatformStorage – сервис поиска рекламных площадок по регионам

Этот репозиторий содержит веб-сервис для быстрого поиска рекламных площадок, работающих в заданном регионе. Данные загружаются из текстового файла и хранятся в оперативной памяти, обеспечивая мгновенный доступ к информации.

# Функциональность

Хранилище организовано через структуру Trie-дерева ( Bor )

Может быть далеко не оптимально для огромных данных размером 20 000 000 - нужно добавлять параллелизм

поддерживает In-memory для быстрого доступа к данным

Сервис тестировался через Postman

# Примеры API-запросов

POST /api/adplatforms/upload (Принимает .txt файл)

GET /api/query/platforms ( принимает модель, включающую ресурс )



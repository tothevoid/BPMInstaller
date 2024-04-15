# Инструмент автоматизации развёртывания дистрибутива BPMSoft в режиме разработки

На текущий момент приложение ориентировано только на развёртывание версий дистрибутивов под .NET Core

Инструмент может быть полезен для быстрого развёртывания стенда при следующих возможных сценариях:
* смена проекта;
* воспроизведение бага на чистой сборке;
* обновление целевой версии дистрибутива на проекте.

Поддерживаемые СУБД:
* PostgreSQL
* SQL Server

Способ взаимодействия:
* Desktop-ное приложение (Windows 10/11 через WPF)
* Web-приложение (TBD)
* CLI (TBD)

Для установки необходимы следующие составляющие:
1. Redis

    Варианты:
    * установить [портированную версии](https://github.com/microsoftarchive/redis/releases);
    * использовать развёрнутый на доступном удалённом хосте;
    * развернуть с помощью docker. Например c помощью: `docker run --name bpm-redis -d redis`.
2.  Необходимая СУБД
    
    Варианты:
    * установка [Sql server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) или [Postgres](https://www.postgresql.org/download/)
    * развёрнутая на доступном удалённом хосте (в данном случае восстановление бекапа не будет работать);
    * развернуть с помощью docker

3.  Распакованный дистрибутив

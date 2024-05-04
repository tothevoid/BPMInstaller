namespace BPMInstaller.Core.Resources
{
    public static class InstallationResources
    {
        public static class MainWorkflow
        {
            public const string Started = "Запущена установка приложения";

            public const string Ended = "Установка приложения завершена";
        }

        public static class ConnectionStrings
        {
            public const string Actualization = "Актуализация ConnectionStrings";

            public const string Actualized = "ConnectionStrings актулизированы";
        }

        public static class Distributive
        {
            public const string PortActualization = "Актуализация порта приложения";

            public const string PortActualized = "Порт актуализирован";

            public const string FixingCookies = "Исправление авторизации";

            public const string CookiesFixed = "Авторизация исправлена";
        }

        public static class Application
        {
            public const string Starting = "Приложение запускается";

            public const string Started = "Приложение запущено";

            public const string Compiling = "Запущена компиляция приложения";

            public static class Instance
            {
                public const string Validation = "Проверка наличия запущенного приложения";

                public const string ThereIsNoActiveInstance = "Активное приложение не найдено";

                public const string Terminated = "Закрыто активное приложение";
            }
        }

        public static class Redis
        {
            public const string Validating = "Валидация подключения к Redis";

            public const string Failed = "Ошибка подключения к Redis";

            public const string Success = "Успешное подключение к Redis";

            public const string Flushing = "Запущена очистка кэша Redis";

            public const string Flushed = "Кэша Redis очищен";
        }

        public static class ForcePasswordChange
        {
            public const string Fixing = "Исправление принудительной смены пароля";

            public const string Fixed = "Принудительной смена пароля отлючена";
        }

        public static class Licensing
        {
            public const string Started = "Процедура добавления лицензий начата";

            public const string CidActualization = "Обновление CId";

            public const string CidActualized = "CId обновлён";

            public const string Applying = "Установка лицензий";

            public const string Applied = "Лицензии установлены";

            public const string AssigningTo = "Назначение лицензий на {0}";

            public const string Assigned = "Лицензии назначены";

            public const string Ended = "Процедура добавления лицензий завершена";
        }

        public static class Database
        {
            public static class Connection
            {
                public const string Validating = "Валидация подключения к БД";

                public const string Failed = "Ошибка подключения к БД";

                public const string Success = "Успешное подключение к БД";
            }

            public static class OtherConnections
            {
                public const string Disconnecting = "Сброс активных подключений к БД";

                public const string Disconnected = "Активные подключения к БД сброшены";
            }

            public static class Restoration
            {
                public const string EmptyConfig = "Отсутствует конфиг для восстановления БД";

                public const string Started = "Запущено восстановление БД";

                public const string Ended = "Завершено восстановление БД";

                public const string Failed = "Во время восстановления БД возникла ошибка: {0}";

                public const string CopyingBackupFile = "Копирование файла с бекапом";

                public const string BackupFileCopied = "Копирование файла с бекапом";

                public const string FileIsNotCopiedIntoContainer = "Не получилось скопировать файл бекапа в контейнер";

                public const string GeneratingDataMigrationCommand = "Формирование команды переноса данных";

                public const string DataMigrationCommandGenerated = "Формирование команды переноса данных завершено";

                public const string DataMigrationStarted = "Запущен перенос данных из бэкапа";

                public const string DataMigrationEnded = "Перенос данных из бэкапа завершён";

                public static class SqlServer
                {
                    public const string BackupPartsExtractionStarted = "Запущена загрузка сооставляющих частей бэкапа";

                    public const string BackupPartsExtractionEnded = "Загрузка сооставляющих частей бэкапа завершена";
                }
            }

            public static class Creation
            {
                public const string Started = "Запущено создание БД";

                public const string Done = "БД создана";

                public const string Failed = "Во время создания БД возникла ошибка: {0}";
            }
        }
    }
}

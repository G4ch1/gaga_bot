CREATE TABLE [dbo].[User] (
    [id]   BIGINT IDENTITY (1, 1) NOT NULL,
    [coin] INT  NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([id] ASC)
);
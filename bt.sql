CREATE TABLE users
(
	username NVARCHAR(40) PRIMARY KEY,
	password NVARCHAR(40) NOT NULL
)

CREATE TABLE torrents
(
	id INT IDENTITY(1,1),
	sha NVARCHAR(40) PRIMARY KEY,
	name TEXT NOT NULL,
	date datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	downloaded INT NOT NULL DEFAULT(0),
	username NVARCHAR(40) NOT NULL FOREIGN KEY REFERENCES users(username) ON DELETE CASCADE,
)

CREATE TABLE peers
(
	sha NVARCHAR(40) FOREIGN KEY REFERENCES torrents(sha) ON DELETE CASCADE,
	date datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
	ip NVARCHAR(255) NOT NULL,
	port int NOT NULL,
	peer_id NVARCHAR(40) NOT NULL,
	uploaded bigint NOT NULL,
	downloaded bigint NOT NULL,
	dl_left bigint NOT NULL,

	CONSTRAINT PK_UserGroup PRIMARY KEY NONCLUSTERED (sha, ip, port)
)


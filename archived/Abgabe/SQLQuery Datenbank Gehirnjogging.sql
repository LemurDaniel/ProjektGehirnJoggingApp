drop table Highscore
drop table Spiel
drop table imageNutzer;
drop table Nutzer;


create table Nutzer (
	id int primary key Identity(0,1),
	nickname nvarchar(50) unique not null,
	pwd nvarchar(80) not null,
	token char(50),
	tokenAge datetime,
	blob nvarchar(100)
)
Insert into Nutzer (nickname, pwd) values
('TOM_TESTER', 'TOM_TESTER_TESTET_DINGE')

create table imageNutzer(
	id_nutzer int references Nutzer,
	nameOfFile nvarchar(100) not null
)

create table Spiel (
	id int primary key,
	spiel nvarchar(50) unique not null,
	globaleBestzeit int not null default 2147483647 , 
	globalerDurchschnitt int not null default 0,
	id_Nutzer int references Nutzer not null default 0,
	rekordhalter nvarchar(50) not null default 'TOM_TESTER',
)

create table Highscore(
	id_Nutzer int references Nutzer,
	id_Spiel int references Spiel,
	bestzeit int not null  default 2147483647,
	schlechteste_Zeit int not null default 0,
	durchschnitt int not null default 0,
	anzahl int not null default 0,
	kumulierteSpielzeit bigint not null default 0,
	primary key(id_Nutzer, id_Spiel),
)


GO


CREATE TRIGGER dbo.AktualisiereBestzeit
   ON  dbo.Highscore
   AFTER Update
AS
	Declare @globBest int
	Set @globBest = (Select bestzeit from inserted)

	Declare @globBestNow int
	Set @globBestNow = (Select globaleBestzeit from Spiel where id=(Select id_Spiel from inserted))

	Declare @Nid int
	Set @Nid = (Select id_Nutzer from inserted)
	Declare @Rekordhalter nvarchar(50)
	Set @Rekordhalter = (Select nickname from Nutzer where id=@Nid);

		If (@globBest < @globBestNow)
	Begin
		Update Spiel Set rekordhalter=@Rekordhalter, id_Nutzer=@Nid, globaleBestzeit=@globBest where id=(Select id_Spiel from inserted)
	End

	Declare @globDurchschnitt int
	Set @globDurchschnitt = (Select AVG(durchschnitt) from Highscore where id_Spiel=(Select id_Spiel from inserted) and durchschnitt > 0)

	Update Spiel Set globalerDurchschnitt=@globDurchschnitt where id=(Select id_Spiel from inserted)

GO

CREATE TRIGGER dbo.LegeHighscoresAn
   ON  dbo.Nutzer
   AFTER Insert
AS
	Insert into Highscore (id_spiel, id_nutzer)  Select id, (Select id from inserted) from Spiel
GO



Insert into Spiel (id, spiel) values
(0, 'Schiebepuzzle 3x3'),
(1, 'Schiebepuzzle 4x4'),
(2, 'Schiebepuzzle 5x5'),
(3, '? Farbe == Wort ?'),
(4, 'Kopfrechnen'),
(5, 'Wechselgeld'),
(6, 'Buchstabensalat'),
(7, 'Memorize'),
(8, 'Concentrate'),

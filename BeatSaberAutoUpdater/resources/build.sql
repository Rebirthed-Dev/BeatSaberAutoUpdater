CREATE TABLE IF NOT EXISTS UserData (
	UserID integer PRIMARY KEY,
	previouslyCheckedScore integer DEFAULT 0,
	oldPP decimal DEFAULT 0.0,
	oldGlobalRank integer DEFAULT 0,
	oldCountryRank integer DEFAULT 0,
	oldAverageAccuracy decimal DEFAULT 0.0,
	channelToPostIn integer,
	threadToPostIn integer
);
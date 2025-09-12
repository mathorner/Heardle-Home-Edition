We're creating a app based on the Heardle music quiz. The user hears snippets from songs and has to guess the song before their turns run out. They have five turns.
I want this version of Heardle (Heardle Home Edition) to select songs from the user's song library. This will be a library of audio files such as MP3s on a local network folder.
These are my requirements for the app:
- the backend should be built with .net9 C#
- the frontend should be built in React and TypeScript
- the backend will require access to a network share
- the frontend must be able to play audio files
- no database required for MVP as we will not be keeping track of scores

## MVP User Journey Spec

1. Library Scan
	•	On first run, user selects a folder containing MP3 files.
	•	System scans the folder:
	•	Extracts basic metadata: title, artist, file path.
	•	Builds an index for fast lookup.
	•	No filtering (all MP3s are eligible, even with messy metadata).

2. Game Start
	•	User presses Start Game.
	•	System randomly selects one track from the library.

3. Round Flow
	3.1.	Snippet Reveal
	•	The game starts at the very beginning of the selected track.
	•	Plays progressively longer snippets per attempt:
	•	Attempt 1: 1 second
	•	Attempt 2: 2 seconds
	•	Attempt 3: 4 seconds
	•	Attempt 4: 7 seconds
	•	Attempt 5: 11 seconds
	•	Attempt 6: 16 seconds
	3.2.	Guess Input
	•	After each snippet, user can:
	•	Enter their guess (free text).
	•	Or press Skip to move to the next attempt.
	3.3.	Validation
	•	User’s guess is checked against the track’s metadata:
	•	Correct if title and artist match exactly.
	•	Case-insensitive, ignores punctuation.
	•	If correct:
	•	Display “Correct! [Title – Artist]”.
	•	End the round.
	3.4.	Next Attempt
	•	If incorrect or skipped:
	•	Move to next snippet length.
	•	Up to a maximum of 6 attempts.

4. Round End
	•	If correct: show success message and play full track.
	•	If all 6 attempts fail: reveal track title and artist.
	•	Option to Play Again (new random track).

⸻

MVP Acceptance Criteria
	•	User can point app at a folder of MP3s and start a game.
	•	App always selects one random track.
	•	App plays exactly the defined snippet lengths.
	•	User has at most 6 attempts to guess.
	•	Correct guess ends the round immediately.
	•	At the end of 6 attempts, the correct answer is revealed.
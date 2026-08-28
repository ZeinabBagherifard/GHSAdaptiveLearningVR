# GHS Adaptive Learning VR

This is a research prototype for training people to recognize GHS (Globally Harmonized System) hazard symbols, using an adaptive, AI-personalized approach rather than a one-size-fits-all course.

## What's in this repo

- The Unity project (the actual training app)
- ghs-gemini-proxy — a small Python backend that talks to Google's Gemini API to generate personalized explanations

## Main files

**Unity scripts (in Assets/Scripts):**
- SessionManager.cs — controls the overall flow: Pre-Assessment, Training, Final Check, and results
- QuizUIManager.cs — handles all the on-screen UI: questions, answers, feedback, and the training comparison screens
- KnowledgeStateManager.cs — keeps track of what each trainee knows, what they got wrong, and which symbol they confused it with
- GHSDataLoader.cs — loads the hazard symbol data from ghs_symbols.json and looks things up when needed
- GeminiProxyClient.cs — sends requests to the Python backend and gets the AI explanation back
- ghs_symbols.json — the actual hazard symbol data: names, meanings, safety tips, and quiz answer options

**ghs-gemini-proxy:**
- main.py — the FastAPI server. Has one endpoint that builds a prompt from the symbol data Unity sends it, asks Gemini for a short comparison, and sends the explanation back
- requirements.txt — the Python packages needed to run the server

## How the training works

A trainee first goes through a quick quiz covering all 7 hazard symbols. Based on what they got wrong, the app builds a personalized training session covering only those symbols. If someone mixes up two symbols (for example, picking "Corrosive" when the question was about "Toxic"), the app detects that specific mix-up and asks Gemini to generate a short, personalized explanation comparing the two symbols — using only the verified facts already stored in the app, never inventing new information. After training, the trainee is meant to be re-quizzed on just those symbols to see if it actually helped, and get a final pass/fail result.

## Why it's built this way

The AI is never allowed to decide what's correct or make up safety information. All of that stays in Unity, sourced from a JSON file of verified hazard data. Gemini's only job is to phrase a comparison between two things Unity already knows to be true. This keeps the safety-critical content trustworthy while still letting each trainee get an explanation tailored to their actual mistake.

## Running it

To run the Unity app, just open the project in Unity and press Play.

To run the AI backend, open a terminal in the ghs-gemini-proxy folder and install the requirements listed in requirements.txt. You'll also need to create a file called .env in that folder containing your own Gemini API key, like this: GEMINI_API_KEY=your_key_here. Once that's set up, start the server with: uvicorn main:app --reload

The server runs locally and Unity connects to it automatically. If the server isn't running, the app still works — it just shows the standard information without the AI-personalized explanation.

## Where things stand

Pre-Assessment and Training are working well, including the AI-driven misconception detection and personalized explanations. The Final Check (the re-quiz after training) is still being worked on and isn't fully reliable yet.

Next steps focus on making the training experience more interactive and responsive to what each trainee actually does — for example, recognizing when someone picks the same wrong answer across multiple symbols, and adjusting the explanation or approach based on that pattern rather than treating every mistake the same way.
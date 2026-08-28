import os
from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from google import genai
from pydantic import BaseModel

load_dotenv()

api_key = os.getenv("GEMINI_API_KEY")
if not api_key:
    raise RuntimeError("GEMINI_API_KEY was not found. Check your .env file.")

client = genai.Client(api_key=api_key)

app = FastAPI(
    title="GHS Gemini Proxy",
    description="Secure proxy between Unity and the Gemini API. "
                "Answer correctness is always checked in Unity. "
                "Authoritative meanings and safety tips are supplied by Unity — "
                "Gemini only personalises the wording.",
)

MODEL = "gemini-3.6-flash"


class ExplanationRequest(BaseModel):
    symbol: str
    wrong_answer: str = ""
    correct_meaning: str
    safety_tip: str
    confused_symbol: str = ""
    confused_meaning: str = ""


class SummaryRequest(BaseModel):
    known_before: list[str]
    learned: list[str]
    still_struggling: list[str]


@app.get("/")
def home():
    return {"status": "GHS Gemini proxy is running"}


@app.post("/explain")
def explain_symbol(request: ExplanationRequest):

    if not request.symbol.strip():
        raise HTTPException(
            status_code=400,
            detail="Field 'symbol' must be non-empty."
        )

    if not request.correct_meaning.strip() or not request.safety_tip.strip():
        raise HTTPException(
            status_code=400,
            detail="Fields 'correct_meaning' and 'safety_tip' must be non-empty."
        )

    if not (request.confused_symbol and request.confused_meaning):
        return {"symbol": request.symbol, "explanation": ""}

    prompt = f"""
You are a workplace safety trainer.

A trainee confused the "{request.symbol}" GHS hazard symbol with the
"{request.confused_symbol}" GHS hazard symbol.

Authoritative meaning of "{request.symbol}":
{request.correct_meaning}

Authoritative meaning of "{request.confused_symbol}":
{request.confused_meaning}

Using only the two authoritative meanings above, explain the clearest
difference between these symbols in exactly two short sentences,
always starting with "The {request.symbol} symbol...".

Use simple English suitable for a new employee.
Do not introduce additional hazards, safety claims, protective equipment,
or emergency actions.
"""

    try:
        response = client.models.generate_content(
            model=MODEL,
            contents=prompt,
        )

        return {
            "symbol": request.symbol,
            "explanation": response.text,
        }

    except Exception as error:
        print(f"Gemini error: {error}")
        raise HTTPException(
            status_code=500,
            detail="The Gemini request failed. Check the server terminal.",
        ) from error


@app.post("/summarize")
def summarize_session(request: SummaryRequest):

    prompt = f"""
You are a workplace safety trainer giving end-of-session feedback.

Trainee results:
- Already knew before training: {", ".join(request.known_before) or "none"}
- Learned during this session: {", ".join(request.learned) or "none"}
- Still needs practice: {", ".join(request.still_struggling) or "none"}

Write a short personalised summary of 3 to 4 sentences.
Be encouraging but honest.
Mention the learner's results accurately.
End with one recommendation based on the symbols that still need practice.
Use simple English.
Do not introduce new safety information or safety claims.
"""

    try:
        response = client.models.generate_content(
            model=MODEL,
            contents=prompt,
        )

        return {
            "summary": response.text,
        }

    except Exception as error:
        print(f"Gemini error: {error}")
        raise HTTPException(
            status_code=500,
            detail="The Gemini request failed. Check the server terminal.",
        ) from error
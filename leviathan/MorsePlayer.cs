using UnityEngine;

internal class MorsePlayer
{
	private const float m_unitLength = 0.05f;

	private string m_text = string.Empty;

	private int m_currentCharacter;

	private string m_characterMorse = string.Empty;

	private int m_characterIndex;

	private float m_time;

	private float m_totalTime;

	private float m_shortMark = 0.05f;

	private float m_longMark = 0.15f;

	private float m_interGap = 0.05f;

	private float m_shortGap = 0.15f;

	private float m_mediumGap = 0.35f;

	private GameObject m_dit;

	private GameObject m_dah;

	public void SetText(string text)
	{
	}

	private string GetMorseCode(char c)
	{
		return c switch
		{
			'A' => "· –", 
			'B' => "– · · ·", 
			'C' => "– · – ·", 
			'D' => "– · ·", 
			'E' => "·", 
			'F' => "· · – ·", 
			'G' => "– – ·", 
			'H' => "· · · ·", 
			'I' => "· ·", 
			'J' => "· – – –", 
			'K' => "– · –", 
			'L' => "· – · ·", 
			'M' => "– –", 
			'N' => "– ·", 
			'O' => "– – –", 
			'P' => "· – – ·", 
			'Q' => "– – · –", 
			'R' => "· – ·", 
			'S' => "· · ·", 
			'T' => "–", 
			'U' => "· · –", 
			'V' => "· · · –", 
			'W' => "· – –", 
			'X' => "– · · –", 
			'Y' => "– · – –", 
			'Z' => "– – · ·", 
			'.' => "· – · – · –", 
			_ => "··", 
		};
	}

	public void Update()
	{
		m_totalTime += Time.deltaTime;
		if (m_text.Length == 0)
		{
			return;
		}
		m_time -= Time.deltaTime;
		if (m_time >= 0f)
		{
			return;
		}
		m_time = 0.05f;
		if (m_characterMorse.Length == 0)
		{
			if (m_currentCharacter == m_text.Length)
			{
				m_text = string.Empty;
				return;
			}
			char c = m_text[m_currentCharacter];
			m_currentCharacter++;
			if (c == ' ')
			{
				m_time = m_mediumGap;
				return;
			}
			m_characterMorse = GetMorseCode(c);
			m_characterIndex = 0;
			PLog.Log("MorsePlayer.Update: " + m_characterMorse);
			m_time = m_shortGap;
			return;
		}
		char c2 = m_characterMorse[m_characterIndex];
		switch (c2)
		{
		case ' ':
			m_time = m_interGap;
			m_characterIndex++;
			return;
		case '·':
			PLog.Log("· " + m_totalTime);
			m_dit.GetComponent<AudioSource>().Play();
			m_time = m_shortMark;
			break;
		}
		if (c2 == '–')
		{
			PLog.Log("– " + m_totalTime);
			m_dah.GetComponent<AudioSource>().Play();
			m_time = m_longMark;
		}
		m_characterIndex++;
		if (m_characterIndex == m_characterMorse.Length)
		{
			m_characterMorse = string.Empty;
		}
		else
		{
			PLog.Log("Time: " + m_time);
		}
	}
}

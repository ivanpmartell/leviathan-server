using UnityEngine;

internal class OptionsWindow
{
	private bool m_inGame;

	private bool m_bloom;

	private bool m_aa;

	private bool m_jazzyGraphic;

	private string m_customVSMusic;

	private string m_vO;

	private GameObject m_guiCamera;

	private GameObject m_optionsGui;

	private float m_oldMusicVolume;

	private float m_oldSfxVolume;

	private bool m_oldBloom;

	private bool m_oldAA;

	private bool m_oldJazzyGraphic;

	private string m_oldCustomVSMusic;

	private string m_oldVO;

	public OptionsWindow(GameObject guiCamera, bool inGame)
	{
		m_guiCamera = guiCamera;
		m_inGame = inGame;
		m_optionsGui = GuiUtils.CreateGui("OptionsWindow", m_guiCamera);
		m_oldMusicVolume = MusicManager.instance.GetVolume();
		m_oldSfxVolume = AudioManager.instance.GetVolume();
		m_bloom = (m_oldBloom = PostEffector.IsBloomEnabled());
		m_aa = (m_oldAA = PostEffector.IsFXAAEnabled());
		m_jazzyGraphic = (m_oldJazzyGraphic = PlayerPrefs.GetInt("JazzyMode") != 0);
		m_customVSMusic = (m_oldCustomVSMusic = PlayerPrefs.GetString("CustomVSMusic"));
		m_vO = (m_oldVO = PlayerPrefs.GetString("VO"));
		GuiUtils.FindChildOf(m_optionsGui, "MusicVolumeSlider").GetComponent<UISlider>().defaultValue = m_oldMusicVolume;
		GuiUtils.FindChildOf(m_optionsGui, "SfxVolumeSlider").GetComponent<UISlider>().defaultValue = m_oldSfxVolume;
		RefreshProgressBars();
		GuiUtils.FindChildOf(m_optionsGui, "BloomCheckbox").GetComponent<UIStateToggleBtn>().SetState((!m_oldBloom) ? 1 : 0);
		GuiUtils.FindChildOf(m_optionsGui, "AACheckbox").GetComponent<UIStateToggleBtn>().SetState((!m_oldAA) ? 1 : 0);
		GuiUtils.FindChildOf(m_optionsGui, "JazzGraphicsCheckbox").GetComponent<UIStateToggleBtn>().SetState((!m_oldJazzyGraphic) ? 1 : 0);
		GuiUtils.FindChildOf(m_optionsGui, "JazzTrackCheckbox").GetComponent<UIStateToggleBtn>().SetState((m_oldCustomVSMusic.Length == 0) ? 1 : 0);
		GuiUtils.FindChildOf(m_optionsGui, "JazzCommentaryCheckbox").GetComponent<UIStateToggleBtn>().SetState((m_oldVO.Length == 0) ? 1 : 0);
		if (inGame)
		{
			GuiUtils.FindChildOf(m_optionsGui, "JazzGraphicsCheckbox").GetComponent<UIStateToggleBtn>().controlIsEnabled = false;
			if (m_jazzyGraphic)
			{
				GuiUtils.FindChildOf(m_optionsGui, "JazzGraphicsCheckbox").GetComponent<UIStateToggleBtn>().SetState(2);
			}
			else
			{
				GuiUtils.FindChildOf(m_optionsGui, "JazzGraphicsCheckbox").GetComponent<UIStateToggleBtn>().SetState(3);
			}
			GuiUtils.FindChildOf(m_optionsGui, "JazzTrackCheckbox").GetComponent<UIStateToggleBtn>().controlIsEnabled = false;
			if (m_customVSMusic.Length != 0)
			{
				GuiUtils.FindChildOf(m_optionsGui, "JazzTrackCheckbox").GetComponent<UIStateToggleBtn>().SetState(2);
			}
			else
			{
				GuiUtils.FindChildOf(m_optionsGui, "JazzTrackCheckbox").GetComponent<UIStateToggleBtn>().SetState(3);
			}
			GuiUtils.FindChildOf(m_optionsGui, "JazzCommentaryCheckbox").GetComponent<UIStateToggleBtn>().controlIsEnabled = false;
			if (m_vO.Length != 0)
			{
				GuiUtils.FindChildOf(m_optionsGui, "JazzCommentaryCheckbox").GetComponent<UIStateToggleBtn>().SetState(2);
			}
			else
			{
				GuiUtils.FindChildOf(m_optionsGui, "JazzCommentaryCheckbox").GetComponent<UIStateToggleBtn>().SetState(3);
			}
			GuiUtils.FindChildOf(m_optionsGui, "JazzButton").GetComponent<UIButton>().controlIsEnabled = false;
		}
		GuiUtils.FindChildOf(m_optionsGui, "CancelButton").GetComponent<UIButton>().SetValueChangedDelegate(OnCancelPressed);
		GuiUtils.FindChildOf(m_optionsGui, "ApplyButton").GetComponent<UIButton>().SetValueChangedDelegate(OnApplyPressed);
		GuiUtils.FindChildOf(m_optionsGui, "MusicVolumeSlider").GetComponent<UISlider>().SetValueChangedDelegate(OnMusicVolume);
		GuiUtils.FindChildOf(m_optionsGui, "SfxVolumeSlider").GetComponent<UISlider>().SetValueChangedDelegate(OnSFXVolume);
		GuiUtils.FindChildOf(m_optionsGui, "BloomCheckbox").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(OnGraphicBloom);
		GuiUtils.FindChildOf(m_optionsGui, "AACheckbox").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(OnGraphicAA);
		GuiUtils.FindChildOf(m_optionsGui, "JazzGraphicsCheckbox").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(OnJazzGraphic);
		GuiUtils.FindChildOf(m_optionsGui, "JazzTrackCheckbox").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(OnJazzTrack);
		GuiUtils.FindChildOf(m_optionsGui, "JazzCommentaryCheckbox").GetComponent<UIStateToggleBtn>().SetValueChangedDelegate(OnJazzVo);
		GuiUtils.FindChildOf(m_optionsGui, "JazzButton").GetComponent<UIButton>().SetValueChangedDelegate(OnJazzButton);
	}

	public void Close()
	{
		if (m_optionsGui != null)
		{
			Object.Destroy(m_optionsGui);
		}
	}

	private void OnCancelPressed(IUIObject obj)
	{
		MusicManager.instance.SetVolume(m_oldMusicVolume);
		AudioManager.instance.SetVolume(m_oldSfxVolume);
		PostEffector.SetBloomEnabled(m_oldBloom);
		PostEffector.SetFXAAEnabled(m_oldAA);
		Close();
	}

	private void OnApplyPressed(IUIObject obj)
	{
		PlayerPrefs.SetFloat("MusicVolume", MusicManager.instance.GetVolume());
		PlayerPrefs.SetFloat("SfxVolume", AudioManager.instance.GetVolume());
		PlayerPrefs.SetInt("JazzyMode", m_jazzyGraphic ? 1 : 0);
		PlayerPrefs.SetString("CustomVSMusic", m_customVSMusic);
		PlayerPrefs.SetString("VO", m_vO);
		PlayerPrefs.Save();
		PLog.Log("m_jazzyGraphic saved as: " + m_jazzyGraphic);
		Close();
	}

	private void RefreshProgressBars()
	{
		float value = GuiUtils.FindChildOf(m_optionsGui, "MusicVolumeSlider").GetComponent<UISlider>().Value;
		GuiUtils.FindChildOf(m_optionsGui, "MusicValueLabel").GetComponent<SpriteText>().Text = (int)(value * 100f) + "%";
		GuiUtils.FindChildOf(m_optionsGui, "MusicVolumeProgressbar").GetComponent<UIProgressBar>().Value = value;
		float value2 = GuiUtils.FindChildOf(m_optionsGui, "SfxVolumeSlider").GetComponent<UISlider>().Value;
		GuiUtils.FindChildOf(m_optionsGui, "SfxValueLabel").GetComponent<SpriteText>().Text = (int)(value2 * 100f) + "%";
		GuiUtils.FindChildOf(m_optionsGui, "SfxVolumeProgressbar").GetComponent<UIProgressBar>().Value = value2;
	}

	private void OnMusicVolume(IUIObject obj)
	{
		float value = GuiUtils.FindChildOf(m_optionsGui, "MusicVolumeSlider").GetComponent<UISlider>().Value;
		MusicManager.instance.SetVolume(value);
		RefreshProgressBars();
	}

	private void OnSFXVolume(IUIObject obj)
	{
		float value = GuiUtils.FindChildOf(m_optionsGui, "SfxVolumeSlider").GetComponent<UISlider>().Value;
		AudioManager.instance.SetVolume(value);
		RefreshProgressBars();
	}

	private void OnGraphicBloom(IUIObject obj)
	{
		m_bloom = !m_bloom;
		GuiUtils.FindChildOf(m_optionsGui, "BloomCheckbox").GetComponent<UIStateToggleBtn>().SetState((!m_bloom) ? 1 : 0);
		PostEffector.SetBloomEnabled(m_bloom);
	}

	private void OnGraphicAA(IUIObject obj)
	{
		m_aa = !m_aa;
		GuiUtils.FindChildOf(m_optionsGui, "AACheckbox").GetComponent<UIStateToggleBtn>().SetState((!m_aa) ? 1 : 0);
		PostEffector.SetFXAAEnabled(m_aa);
	}

	private void OnJazzGraphic(IUIObject obj)
	{
		m_jazzyGraphic = !m_jazzyGraphic;
		GuiUtils.FindChildOf(m_optionsGui, "JazzGraphicsCheckbox").GetComponent<UIStateToggleBtn>().SetState((!m_jazzyGraphic) ? 1 : 0);
		PLog.Log("m_jazzyGraphic changed to: " + m_jazzyGraphic);
	}

	private void OnJazzTrack(IUIObject obj)
	{
		if (m_customVSMusic.Length == 0)
		{
			m_customVSMusic = "jazzy";
		}
		else
		{
			m_customVSMusic = string.Empty;
		}
		GuiUtils.FindChildOf(m_optionsGui, "JazzTrackCheckbox").GetComponent<UIStateToggleBtn>().SetState((m_customVSMusic.Length == 0) ? 1 : 0);
	}

	private void OnJazzVo(IUIObject obj)
	{
		if (m_vO.Length == 0)
		{
			m_vO = "JazzyBoatman";
		}
		else
		{
			m_vO = string.Empty;
		}
		GuiUtils.FindChildOf(m_optionsGui, "JazzCommentaryCheckbox").GetComponent<UIStateToggleBtn>().SetState((m_vO.Length == 0) ? 1 : 0);
	}

	private void OnJazzButton(IUIObject obj)
	{
		m_jazzyGraphic = false;
		m_customVSMusic = string.Empty;
		m_vO = string.Empty;
		OnJazzGraphic(null);
		OnJazzTrack(null);
		OnJazzVo(null);
	}
}

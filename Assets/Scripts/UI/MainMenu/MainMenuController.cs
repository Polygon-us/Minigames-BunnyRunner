using Source.Handlers;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;
using UI.Views;

namespace UI.Controllers
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private LoginMenu loginPanel;
        [SerializeField] private RegisterMenu registerPanel;
        [SerializeField] private MainMenu mainMenuPanel;
        
        private ViewBase _currentMenu;

        private void Start()
        {
            loginPanel.GoToRegister += ShowRegister;
            loginPanel.OnLoginSuccess += StartGame;
            registerPanel.GoToLogin += ShowLogin;
            registerPanel.OnRegisterSuccess += StartGame;
            
            registerPanel.gameObject.SetActive(false);
            mainMenuPanel.gameObject.SetActive(false);
            
            ShowLogin();
        }

        private void ShowPanel(ViewBase panel)
        {
            _currentMenu?.gameObject.SetActive(false);
            _currentMenu?.OnHide();
            _currentMenu = panel;
            _currentMenu.gameObject.SetActive(true);
            _currentMenu.OnShow();
            
            eventSystem.SetSelectedGameObject(_currentMenu.FirstSelected);
        }

        private void ShowLogin()
        {
            ShowPanel(loginPanel);
        }

        private void ShowRegister()
        {
            ShowPanel(registerPanel);
        }

        private void ShowMainMenu()
        {
            ShowPanel(mainMenuPanel);
        }
        
        private void StartGame()
        {
            loginPanel.GoToRegister -= ShowRegister;
            loginPanel.OnLoginSuccess -= StartGame;
            registerPanel.GoToLogin -= ShowLogin;
            registerPanel.OnRegisterSuccess -= StartGame;

            SceneManager.LoadScene("Main");
        }

        [ContextMenu("ResetSaveGame")]
        private void ResetSaveGame()
        {
            BaseHandler.ResetInfoFromPrefs();
        }
    }
}
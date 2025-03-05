using Source.Utils.Validations;
using Source.Utils.Responses;
using Source.DTOs.Response;
using Source.DTOs.Request;
using Source.Handlers;
using UnityEngine.UI;
using Source.Popups;
using UnityEngine;
using Source.DTOs;
using UnityREST;
using UI.DTOs;
using System;
using Source.Utils.JWT;
using TMPro;

namespace UI.Views
{
    public class LoginMenu : ViewBase
    {
        // [SerializeField] private Button registerButton;
        [SerializeField] private Button sendButton;

        [SerializeField] private TMP_InputField emailInputField;
        // [SerializeField] private TMP_InputField passwordInputField;

        public Action OnLoginSuccess;
        public Action GoToRegister;

        protected override void OnCreation()
        {
            sendButton.onClick.AddListener(OnSendLogin);
            // registerButton.onClick.AddListener(() => GoToRegister?.Invoke());

            // Buttons.Add(registerButton);
            Buttons.Add(sendButton);

            SetPrefills();
        }

        public override void OnShow()
        {
        }

        public override void OnHide()
        {
        }

        private void OnSendLogin()
        {
            ToggleButtons(false);

            LoginDto loginDto = new LoginDto
            {
                email = emailInputField.text,
                acceptedTerms = true
                // password = passwordInputField.text
            };

            ResultResponse<LoginDto> validate = LoginValidation.Validate(loginDto);

            if (!validate.IsSuccess)
            {
                ConfirmationPopUp.Instance.Open(validate.ErrorMessage);
                ToggleButtons(true);
                return;
            }

            LoginHandler.Login(validate.Data, OnLoginResponse);
        }

        private void OnLoginResponse(WebResult<ResponseDto<LoginResponseDto>> response)
        {
            if (response.result.HasError())
            {
                ConfirmationPopUp.Instance.Open(response.data.error);
                ToggleButtons(true);
                return;
            }
 
            JWTPayloadDto payloadDto = JsonWebToken.DecodeToObject<JWTPayloadDto>
            (
                response.data.data.token, 
                string.Empty, 
                false
            );

            int.TryParse(payloadDto.score, out int score);

            int.TryParse(payloadDto.distance, out int distance);
            
            SaveUserInfoDto saveUserInfoDto = BaseHandler.SaveUserInfo;
            
            BaseHandler.SaveInfoToPrefs(payloadDto.username, payloadDto.email, score, distance, saveUserInfoDto?.tutorial ?? true/*, passwordInputField.text*/);
            
            RestApiManager.Instance.SetAuthToken(response.data.data.token);

            OnLoginSuccess?.Invoke();
        }

        private void SetPrefills()
        {
            SaveUserInfoDto saveUserInfoDto = BaseHandler.SaveUserInfo;

            if (saveUserInfoDto == null)
                return;

            emailInputField.text = saveUserInfoDto.email;
            // passwordInputField.text = saveUserInfoDto.password;
        }
    }
}
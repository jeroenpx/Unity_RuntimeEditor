using UnityEngine.UI;
using UnityEngine;

using System.Collections.Generic;
using System.Collections;
using System.Linq;

using Battlehub.RTCommon;

namespace Battlehub.Cubeman
{
    public class CubemenGame : MonoBehaviour
    {
        public Text TxtScore;
        public Text TxtCompleted;
        public Text TxtTip;
        public Button BtnReplay;
        public GameObject GameUI;

        private int m_score;
        private int m_total;
        private bool m_gameOver;

        [SerializeField]
        private GameCharacter[] m_storedCharacters;
        private GameCharacter m_current;
        private List<GameCharacter> m_activeCharacters;
        private GameCameraFollow m_playerCamera;

        private IRTE m_rte;
        private IRTEState m_rteState;
        
        private void RuntimeAwake()
        {
        }

        private void RuntimeStart()
        {
            StartGame();
        }

        private void OnRuntimeDestroy()
        {
        }

        private void OnRuntimeActivate()
        {
            enabled = true;

            if(m_current != null)
            {
                m_current.HandleInput = true;
            }
        }

        private void OnRuntimeDeactivate()
        {
            enabled = false;

            if (m_current != null)
            {
                m_current.HandleInput = false;
            }
        }

        private void OnRuntimeEditorOpened()
        {
            StopGame();
        }

        private void OnRuntimeEditorClosed()
        {
            StartGame();
        }

        private void Awake()
        {
            m_rteState = IOC.Resolve<IRTEState>();
            if(m_rteState == null)
            {
                Debug.LogWarning("Unable to resolve IRTEState");
                Destroy(gameObject);
                return;
            }

            if (BtnReplay != null)
            {
                BtnReplay.onClick.AddListener(RestartGame);
            }
            if (GameUI != null)
            {
                GameUI.SetActive(false);
            }
        
            if (!m_rteState.IsCreated)
            {
                StartGame();
            }        
        }

        private void OnDestroy()
        {
            if(BtnReplay != null)
            {
                BtnReplay.onClick.RemoveListener(RestartGame);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SwitchPlayer(m_current, 0.0f, true);
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                SwitchPlayer(m_current, 0.0f, false);
            }
        }

        private void StartGame()
        {
            if (GameUI != null)
            {
                GameUI.SetActive(true);
            }

            if(m_storedCharacters != null)
            {
                for (int i = 0; i < m_storedCharacters.Length; ++i)
                {
                    GameCharacter storedCharacter = m_storedCharacters[i];
                    if (storedCharacter != null)
                    {
                        Destroy(storedCharacter.gameObject);
                    }
                }
            }
            
            GameCharacter[] characters = FindObjectsOfType<GameCharacter>().Where(g => g.GetComponent<ExposeToEditor>() != null).OrderBy(c => c.name).ToArray();
            for(int i = 0; i < characters.Length; ++i)
            {
                characters[i].Game = this;
                characters[i].IsActive = true;
            }
            SaveCharactersInInitalState(characters);
            InitializeGame(characters);
        }

        private void StopGame()
        {
            RestartGame();

            for(int i = 0; i < m_activeCharacters.Count; ++i)
            {
                GameCharacter character = m_activeCharacters[i];
                character.IsActive = false;
            }

            if(m_playerCamera != null)
            {
                m_playerCamera.target = null;
            }

            if(GameUI != null)
            {
                GameUI.SetActive(false);
            }
        }

        private void RestartGame()
        {
            if (m_activeCharacters != null)
            {
                for (int i = 0; i < m_activeCharacters.Count; ++i)
                {
                    GameCharacter activeCharacter = m_activeCharacters[i];
                    Destroy(activeCharacter.gameObject);
                }
            }
            
            GameCharacter[] characters = m_storedCharacters;
            SaveCharactersInInitalState(characters);
            InitializeGame(characters);
        }

        private void InitializeGame(GameCharacter[] characters)
        {
            m_gameOver = false;
            m_playerCamera = FindObjectOfType<GameCameraFollow>();
            if (m_playerCamera != null)
            {
                Canvas canvas = GetComponentInChildren<Canvas>();
                Camera cam = m_playerCamera.GetComponent<Camera>();
                canvas.worldCamera = cam;
                canvas.planeDistance = cam.nearClipPlane + 0.01f;
            }

            m_activeCharacters = new List<GameCharacter>();
            for (int i = 0; i < characters.Length; ++i)
            {
                GameCharacter character = characters[i];
                character.transform.SetParent(null);
                character.gameObject.SetActive(true);

                ExposeToEditor exposeToEditor = character.GetComponent<ExposeToEditor>();
                if (!exposeToEditor)
                {
                    character.gameObject.AddComponent<ExposeToEditor>();
                }
                else
                {
                    ExposeToEditor[] children = character.GetComponentsInChildren<ExposeToEditor>(true);
                    for (int j = 0; j < children.Length; ++j)
                    {
                        ExposeToEditor child = children[j];
                        child.MarkAsDestroyed = false;
                    }
                }
                m_activeCharacters.Add(character);
            }

            m_total = m_activeCharacters.Count;
            m_score = 0;

            if (m_total == 0)
            {
                TxtCompleted.gameObject.SetActive(true);
                TxtScore.gameObject.SetActive(false);
                TxtTip.gameObject.SetActive(false);

                TxtCompleted.text = "Game Over!";
                m_gameOver = true;
            }
            else
            {
                TxtCompleted.gameObject.SetActive(false);
                TxtScore.gameObject.SetActive(true);
                UpdateScore();
                SwitchPlayer(null, 0.0f, true);
            }
        }

        private void SaveCharactersInInitalState(GameCharacter[] characters)
        {
            GameCharacter[] storedCharacters = new GameCharacter[characters.Length];
            for (int i = 0; i < characters.Length; ++i)
            {
                GameCharacter character = characters[i];
                bool isActive = character.gameObject.activeSelf;
                character.gameObject.SetActive(false);

                GameCharacter stored = Instantiate(character);
                stored.name = character.name;
                character.gameObject.SetActive(isActive);

                ExposeToEditor[] exposeToEditor = stored.GetComponentsInChildren<ExposeToEditor>();
                foreach(ExposeToEditor obj in exposeToEditor)
                {
                    obj.MarkAsDestroyed = true;
                }

                stored.transform.SetParent(transform);
                storedCharacters[i] = stored;
            }

            m_storedCharacters = storedCharacters;
        }

        private void UpdateScore()
        {
            TxtScore.text = "Saved : " + m_score + " / " + m_total;
        }

        private bool IsGameCompleted()
        {
            return m_activeCharacters.Count == 0;
        }

        public void OnPlayerFinish(GameCharacter gameCharacter)
        {
            m_score++;
            UpdateScore();

            SwitchPlayer(gameCharacter, 1.0f, true);
            m_activeCharacters.Remove(gameCharacter);

            if (IsGameCompleted())
            {
                m_gameOver = true;
                TxtTip.gameObject.SetActive(false);
                StartCoroutine(ShowText("Congratulation! \n You have completed a great game "));
            }
        }

        private IEnumerator ShowText(string text)
        {
            yield return new WaitForSeconds(1.5f);
            if (m_gameOver)
            {
                TxtScore.gameObject.SetActive(false);
                TxtCompleted.gameObject.SetActive(true);
                TxtCompleted.text = text;
            }
        }

        public void OnPlayerDie(GameCharacter gameCharacter)
        {
            m_gameOver = true;
            m_activeCharacters.Remove(gameCharacter);
            TxtTip.gameObject.SetActive(false);

            StartCoroutine(ShowText("Game Over!"));
            for (int i = 0; i < m_activeCharacters.Count; ++i)
            {
                m_activeCharacters[i].HandleInput = false;
            }
        }

        public void SwitchPlayer(GameCharacter current, float delay, bool next)
        {
            if (m_gameOver)
            {
                return;
            }

            int index = 0;
            if (current != null)
            {
                current.HandleInput = false;
                index = m_activeCharacters.IndexOf(current);
                if (next)
                {
                    index++;
                    if (index >= m_activeCharacters.Count)
                    {
                        index = 0;
                    }
                }
                else
                {
                    index--;
                    if (index < 0)
                    {
                        index = m_activeCharacters.Count - 1;
                    }
                }
            }

            m_current = m_activeCharacters[index];
            if (current == null)
            {
                ActivatePlayer();
            }
            else
            {
                StartCoroutine(ActivateNextPlayer(delay));
            } 
        }

        IEnumerator ActivateNextPlayer(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (m_gameOver)
            {
                yield break;
            }

            ActivatePlayer();
        }

        private void ActivatePlayer()
        {
            if (m_current != null)
            {
                if(enabled)
                {
                    m_current.HandleInput = true;
                }
            }
            if (m_playerCamera != null)
            {
                m_playerCamera.target = m_current.transform;
                m_current.Camera = m_playerCamera.transform; 
            }
           
        }
    }
}

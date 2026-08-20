using UnityEngine;

public class SomBotao : MonoBehaviour
{
    [Header("Configuração de Áudio")]
    [Tooltip("Arraste o seu arquivo de som (AudioClip) aqui no Inspector")]
    public AudioClip somDoBotao;

    [Range(0f, 1f)]
    [Tooltip("Volume do efeito sonoro")]
    public float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Obtém o componente AudioSource ou cria um automaticamente caso não exista
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Chame este método no evento OnClick do seu botão
    /// </summary>
    public void TocarSom()
    {
        if (somDoBotao != null && audioSource != null)
        {
            audioSource.PlayOneShot(somDoBotao, volume);
        }
        else
        {
            Debug.LogWarning("Nenhum AudioClip atribuído em SomBotao!", this);
        }
    }
}

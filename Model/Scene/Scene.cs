namespace ET;

public class Scene : Singleton<Scene>, ISingletonAwake
{
    private EntityRef<DirectionLightComponent> mainLight;
    public DirectionLightComponent MainLight => this.mainLight;

    private EntityRef<CameraComponent> mainCamera;
    public CameraComponent MainCamera => this.mainCamera;
    
    private EntityRef<GameObject> activeScene;
    public GameObject ActiveScene
    {
        get => this.activeScene;
        set
        {
            GameObject oldValue = this.activeScene;
            oldValue?.Dispose();

            if (value != null)
            {
                this.activeScene = value;
                
                bool findLight = false, findCamera = false;
                foreach (GameObject gameObject in value.Foreach())
                {
                    if (gameObject.GetComponent(out DirectionLightComponent light))
                    {
                        this.mainLight = light;
                        findLight = true;
                    }

                    if (gameObject.GetComponent(out PerspectiveCameraComponent camera))
                    {
                        this.mainCamera = camera;
                        findCamera = true;
                    }

                    if (findLight && findCamera)
                    {
                        break;
                    }
                }
            }
            else
            {
                this.activeScene = null;
                this.mainLight = null;
                this.mainCamera = null;
            }
        }
    }
    
    public void Awake()
    {
        
    }

    protected override void Destroy()
    {
        base.Destroy();

        this.ActiveScene?.Dispose();
        this.ActiveScene = null;
    }
}
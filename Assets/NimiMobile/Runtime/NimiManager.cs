using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEditor;

/*
 * Helper library for fast mobile prototyping
 * Author: Nikola Milanovic
 */ 


namespace NimiMobilePack
{
    public class NimiManager : MonoBehaviour
    {
        public static NimiManager instance;

        public int ApplicationTargetFrameRate = 50;
        public string BundleIdentefier;
        public static Transform Player
        {
            get
            {
                return GameObject.FindGameObjectWithTag("Player").transform;
            }
        }
        private void Awake()
        {
            instance = this;
            Application.targetFrameRate = ApplicationTargetFrameRate;
        }

        void Start()
        {
            
        }

        public static Coroutine BeginCoroutine(IEnumerator coroutine)
        {
            return instance.StartCoroutine(coroutine);
        }

        public void _CreateFolders()
        {
#if UNITY_EDITOR
            AssetDatabase.CreateFolder("Assets", "Animations");
            AssetDatabase.CreateFolder("Assets", "Scripts");
            AssetDatabase.CreateFolder("Assets", "Materials");
            AssetDatabase.CreateFolder("Assets", "Physics Materials");
            AssetDatabase.CreateFolder("Assets", "Fonts");
            AssetDatabase.CreateFolder("Assets", "Models");
            AssetDatabase.CreateFolder("Assets", "Shaders");
            AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets", "Sprites");
            AssetDatabase.CreateFolder("Assets", "Textures");
            AssetDatabase.CreateFolder("Assets", "Resources");
            PlayerSettings.bundleVersion = "com.NimiGames." + BundleIdentefier;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
#endif
        }
    
        void Update()
        {
#if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.R))
            {
                NLevelManager.RestartScene();
            }
#endif
        }
    }


    public enum CameraFollowType { NORMAL, X_AXIS , Y_AXIS , Z_AXIS, XZ_AXIS, XY_AXIS, ZY_AXIS, ALWAYS_BEHIND }

    public class NCameraUtility
    {
        public Transform target { get; set; }
        private Transform cameraTransform;
        public Vector3 offset { get; set; }
        public Camera camera { get; set; }
        private Vector3 move;

        private Vector3 xzDir;
        private Vector3 xyDir;
        private Vector3 zyDir;
        private bool doFollow;
        private Vector3 Velocity;

        private bool isLocked;
        private Coroutine setco;

        /// <summary>
        /// Wrapper for controlling camera follow.
        /// </summary>
        /// <param name="Camera"></param>
        /// <param name="Object to follow"></param>
        public NCameraUtility(Camera camera , Transform followTarget)
        {
            this.camera = camera;
            cameraTransform = camera.transform;
            target = followTarget;
            offset = cameraTransform.position - target.position;
            isLocked = false;
            move = Vector3.zero;
            xzDir = new Vector3(1f, 0f, 1f);
            xyDir = new Vector3(1f, 1f, 0f);
            zyDir = new Vector3(0f, 1f, 1f);
            Velocity = Vector3.one;
        }

        public Vector3 GetWorldPosition(Vector3 screenPosition)
        {
            return camera.ScreenToWorldPoint(screenPosition);
        }

        public Vector3 GetScreenPosition(Vector3 worldPosition)
        {
            return camera.WorldToScreenPoint(worldPosition);
        }

        public Ray GetRay(Vector3 screenPosition)
        {
            return camera.ScreenPointToRay(screenPosition);
        }

        public void SetNewOffset(Vector3 Offset,float Speed)
        {
            if (setco != null) NimiManager.instance.StopCoroutine(setco);

            setco =  NimiManager.instance.StartCoroutine(IESetOffset(Offset,Speed));
        }

        private IEnumerator IESetOffset(Vector3 Offset,float Speed)
        {
            Vector3 temp = offset;

            while(temp != Offset)
            {
                temp = Vector3.MoveTowards(temp, Offset, Time.deltaTime * Speed);
                offset = temp;
                yield return 0;
            }

            setco = null;
        }


        public void IncreaseOffset(float amount)
        {
            NimiManager.instance.StartCoroutine(IESetOffset(amount, true));
        }

        public void DecreaseOffset(float amount)
        {
            NimiManager.instance.StartCoroutine(IESetOffset(amount, false));
        }


        private IEnumerator IESetOffset(float amount,bool increase)
        {

            yield return new WaitUntil(() => !isLocked);
            isLocked = true;

            Vector3 desiredPos = increase ? camera.transform.position + (-camera.transform.forward.normalized * amount) : camera.transform.position + (camera.transform.forward.normalized * amount);

            Vector3 temp = cameraTransform.position;
            
            while(temp != desiredPos)
            {
                temp = Vector3.MoveTowards(temp, desiredPos, Time.deltaTime * 60f);
                cameraTransform.position = temp;
                offset = cameraTransform.position - target.position;
                yield return 0;
            }

            camera.transform.position = desiredPos;
            offset = cameraTransform.position - target.position;
            isLocked = false;
        }

        public void SetFOW(float fow,float speed = 25f,float delay = 0f)
        {
            NimiManager.instance.StartCoroutine(IESetFOW(fow, speed,delay));
        }

        private IEnumerator IESetFOW(float fow,float speed,float delay)
        {

            yield return new WaitForSecondsRealtime(delay);

            while(!NFloatUtility.CompareFloat(fow,camera.fieldOfView))
            {
                camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView,fow,Time.deltaTime * speed);
                yield return 0;
            }

            camera.fieldOfView = fow;

        }

        /// <summary>
        /// Follows the target in a specifed way.Call this from LateUpdate() Method.
        /// </summary>
        /// <param name="A way to follow the target"></param>
        public void Follow(CameraFollowType followType,string axis = "z")
        {
            switch(followType)
            {
                case CameraFollowType.NORMAL:
                    normalFollow();
                    break;
                case CameraFollowType.X_AXIS:
                    follow(Vector3.right);
                    break;
                case CameraFollowType.Y_AXIS:
                    follow(Vector3.up);
                    break;
                case CameraFollowType.Z_AXIS:
                    follow(Vector3.forward);
                    break;
                case CameraFollowType.XY_AXIS:
                    follow(xyDir);
                    break;
                case CameraFollowType.XZ_AXIS:
                    follow(xzDir);
                    break;
                case CameraFollowType.ZY_AXIS:
                    follow(zyDir);
                    break;
                case CameraFollowType.ALWAYS_BEHIND:
                    behind(axis);
                    break;
            }
        }

        /// <summary>
        /// Follows the target in a specifed way.Call this from LateUpdate() Method.
        /// </summary>
        /// <param name="A way to follow the target"></param>
        /// <param name="Follow and look at target?"></param>
        public void Follow(CameraFollowType followType, bool LookAtTarget)
        {
            switch (followType)
            {
                case CameraFollowType.NORMAL:
                    normalFollow();
                    break;
                case CameraFollowType.X_AXIS:
                    follow(Vector3.right);
                    break;
                case CameraFollowType.Y_AXIS:
                    follow(Vector3.up);
                    break;
                case CameraFollowType.Z_AXIS:
                    follow(Vector3.forward);
                    break;
                case CameraFollowType.XY_AXIS:
                    follow(xyDir);
                    break;
                case CameraFollowType.XZ_AXIS:
                    follow(xzDir);
                    break;
                case CameraFollowType.ZY_AXIS:
                    follow(zyDir);
                    break;
                case CameraFollowType.ALWAYS_BEHIND:
                    behind("z");
                    break;
            }

            if (LookAtTarget) cameraTransform.LookAt(target);
        }

        /// <summary>
        /// Follows the target smooth in a specifed way. Call this from LateUpdate() Method. Lower values make camera snap to position faster.
        /// </summary>
        /// <param name="A way to follow the target"></param>
        /// <param name="Smoothing level"></param>
        public void FollowSmoothly(CameraFollowType followType , float Smoothing, string axis)
        {
            switch (followType)
            {
                case CameraFollowType.NORMAL:
                    normalFollow(Smoothing);
                    break;
                case CameraFollowType.X_AXIS:
                    follow(Vector3.right,Smoothing);
                    break;
                case CameraFollowType.Y_AXIS:
                    follow(Vector3.up, Smoothing);
                    break;
                case CameraFollowType.Z_AXIS:
                    follow(Vector3.forward, Smoothing);
                    break;
                case CameraFollowType.XY_AXIS:
                    follow(xyDir, Smoothing);
                    break;
                case CameraFollowType.XZ_AXIS:
                    follow(xzDir, Smoothing);
                    break;
                case CameraFollowType.ZY_AXIS:
                    follow(zyDir, Smoothing);
                    break;
                case CameraFollowType.ALWAYS_BEHIND:
                    behind(axis);
                    break;
            }
        }

        /// <summary>
        /// Follows the target smooth in a specifed way. Call this from LateUpdate() Method. Lower values make camera snap to position faster.
        /// </summary>
        /// <param name="A way to follow the target"></param>
        /// <param name="Smoothing level"></param>
        /// <param name="Follow and look at target?"></param>
        public void FollowSmoothly(CameraFollowType followType, float Smoothing, bool LookAtTarget = false)
        {
            switch (followType)
            {
                case CameraFollowType.NORMAL:
                    normalFollow(Smoothing);
                    break;
                case CameraFollowType.X_AXIS:
                    follow(Vector3.right, Smoothing);
                    break;
                case CameraFollowType.Y_AXIS:
                    follow(Vector3.up, Smoothing);
                    break;
                case CameraFollowType.Z_AXIS:
                    follow(Vector3.forward, Smoothing);
                    break;
                case CameraFollowType.XY_AXIS:
                    follow(xyDir, Smoothing);
                    break;
                case CameraFollowType.XZ_AXIS:
                    follow(xzDir, Smoothing);
                    break;
                case CameraFollowType.ZY_AXIS:
                    follow(zyDir, Smoothing);
                    break;
                case CameraFollowType.ALWAYS_BEHIND:
                    behind("z",Smoothing);
                    break;
            }

            if (LookAtTarget) cameraTransform.LookAt(target);
        }

        public void FollowSpherical(Vector3 SphereCentre , float Distance)
        {
            move = target.position - SphereCentre;
            move += move.normalized * Distance;

            cameraTransform.position = SphereCentre + move;
            cameraTransform.LookAt(target, cameraTransform.up);
        }

        public void FollowSphericalSmoothly(Vector3 SphereCentre, float Distance, float Smooth)
        {
            move = target.position - SphereCentre;
            move += move.normalized * Distance;

            cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, SphereCentre + move, ref Velocity, Smooth);
            cameraTransform.LookAt(target, cameraTransform.up);
        }

        private void normalFollow()
        {
            cameraTransform.position = target.position + offset;
        }

        private void normalFollow(float Smooth)
        {
            cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position,target.position + offset,ref Velocity,Smooth);
        }

        private void follow(Vector3 dir)
        {
            
            move.x = !NFloatUtility.CompareFloat(dir.x, 0f) ? target.position.x + offset.x : cameraTransform.position.x;
            move.y = !NFloatUtility.CompareFloat(dir.y, 0f) ? target.position.y + offset.y : cameraTransform.position.y;
            move.z = !NFloatUtility.CompareFloat(dir.z, 0f) ? target.position.z + offset.z : cameraTransform.position.z;

            cameraTransform.position = move;
        }

        private void follow(Vector3 dir,float smooth)
        {
            move.x = !NFloatUtility.CompareFloat(dir.x, 0f) ? target.position.x + offset.x : cameraTransform.position.x;
            move.y = !NFloatUtility.CompareFloat(dir.y, 0f) ? target.position.y + offset.y : cameraTransform.position.y;
            move.z = !NFloatUtility.CompareFloat(dir.z, 0f) ? target.position.z + offset.z : cameraTransform.position.z;

            cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, move, ref Velocity, smooth);
        }

        private void behind(string axis)
        {
            float set = axis == "x" ? offset.x : offset.z;
            float difference = Mathf.Abs(set);
            move.x = target.position.x - (target.forward.x * difference);
            move.y = target.position.y + offset.y;
            move.z = target.position.z - (target.forward.z * difference);
            cameraTransform.position = move;

            move.Set(cameraTransform.eulerAngles.x, target.eulerAngles.y, cameraTransform.eulerAngles.z);
            cameraTransform.rotation = Quaternion.Euler(move);          
        }

        private void behind(string axis,float Smooth)
        {
            float set = axis == "x" ? offset.x : offset.z;
            float difference = Mathf.Abs(set);
            move.x = target.position.x - (target.forward.x * difference);
            move.y = target.position.y + offset.y;
            move.z = target.position.z - (target.forward.z * difference);
            cameraTransform.position = move;

            move.Set(cameraTransform.eulerAngles.x, target.eulerAngles.y, cameraTransform.eulerAngles.z);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation,Quaternion.Euler(move),Smooth);
        }

    }


    public class SwipeHandler
    {
        public class TouchInfo
        {
            public Vector2 touchPosition;
            public bool swipeComplete;
            public float timeSwipeStarted;
        }

        public int swipeLength;
        public int swipeVariance;
        public float timeToSwipe;
        private TouchInfo[] touchInfoArray;
        private int activeTouch = -1;

        private List<UnityAction> leftActions;
        private List<UnityAction> rightActions;

        //methods
        public SwipeHandler()
        {
            //get a reference to the GUIText component
            touchInfoArray = new TouchInfo[5];
            leftActions = new List<UnityAction>();
            rightActions = new List<UnityAction>();
            swipeLength = 25;
            swipeVariance = 5;
            timeToSwipe = 0.1f;
        }

        public void OnSwipeLeft(UnityAction action)
        {
            leftActions.Add(action);
        }

        public void OnSwipeRight(UnityAction action)
        {
            rightActions.Add(action);
        }

        public void Listen()
        {
            if (Input.touchCount > 0  && Input.touchCount < 6)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touchInfoArray[touch.fingerId] == null)
                        touchInfoArray[touch.fingerId] = new TouchInfo();

                    if (touch.phase == TouchPhase.Began)
                    {
                        touchInfoArray[touch.fingerId].touchPosition = touch.position;
                        touchInfoArray[touch.fingerId].timeSwipeStarted = Time.time;
                    }
                    //check if withing swipe variance      
                    if (touch.position.y > (touchInfoArray[touch.fingerId].touchPosition.y + swipeVariance))
                    {
                        touchInfoArray[touch.fingerId].touchPosition = touch.position;
                    }
                    if (touch.position.y < (touchInfoArray[touch.fingerId].touchPosition.y - swipeVariance))
                    {
                        touchInfoArray[touch.fingerId].touchPosition = touch.position;
                    }
                    //swipe right
                    if ((touch.position.x > touchInfoArray[touch.fingerId].touchPosition.x + swipeLength) && !touchInfoArray[touch.fingerId].swipeComplete &&
                         activeTouch == -1)
                {
                        SwipeComplete("right",touch);
                    }
                    //swipe left
                if ((touch.position.x < touchInfoArray[touch.fingerId].touchPosition.x - swipeLength) && !touchInfoArray[touch.fingerId].swipeComplete &&
                         activeTouch == -1)
                {
                        SwipeComplete("left",touch);
                    }
                    
                if (touch.fingerId == activeTouch && touch.phase == TouchPhase.Ended)
                {

                        foreach (Touch touchReset in Input.touches)
                        {
                            touchInfoArray[touch.fingerId].touchPosition = touchReset.position;
                        }
                        touchInfoArray[touch.fingerId].swipeComplete = false;
                        activeTouch = -1;
                    }
                }
            }
        }

        void SwipeComplete(string dir,Touch touch)
        {
            //Debug.Log(Time.time - touchInfoArray[touch.fingerId].timeSwipeStarted);
            Reset(touch);
            if (timeToSwipe == 0.0f || (timeToSwipe > 0.0f && (Time.time - touchInfoArray[touch.fingerId].timeSwipeStarted) <= timeToSwipe))
            {
                if(dir == "right")
                {
                    foreach(UnityAction action in rightActions)
                    {
                        action.Invoke();
                    }
                }
                else if(dir == "left")
                {
                    foreach (UnityAction action in leftActions)
                    {
                        action.Invoke();
                    }
                }
            }
        }

        void Reset(Touch touch)
        {
            activeTouch = touch.fingerId;
            touchInfoArray[touch.fingerId].swipeComplete = true;
        }
    }


    /// <summary>
    /// Wrapper for input for editor and mobile devices.
    /// </summary>
    public static class NInput
    {
        private static KeyCode Key = KeyCode.Space;
        public static KeyCode key { get { return Key; } set { Key = value; } }

        public static SwipeHandler swipeHandler { get { return new SwipeHandler(); } }

        /// <summary>
        /// Returns true in Editor.Returns true if finger is on mobile screen.
        /// </summary>
        /// <returns></returns>
        public static bool IsScreenTouched()
        {
#if UNITY_EDITOR
            return Input.GetMouseButton(0);
#elif UNITY_ANDROID || UNITY_IOS
            return Input.touchCount > 0;
#endif
        }

        /// <summary>
        /// Gets the first touch that occured on the device.
        /// </summary>
        /// <returns></returns>
        public static Touch GetTouch()
        {
            return Input.GetTouch(0);
        }

        /// <summary>
        /// Returns the value of virtually axis identified by axis name
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static float GetAxis(string axis)
        {
#if UNITY_EDITOR
            return GetEditorAxis(axis);
#elif UNITY_ANDROID || UNITY_IOS
            return GetMobileAxis(axis);
#endif
        }

        private static float GetEditorAxis(string axis)
        {
            return Input.GetAxis(axis);
        }

        private static float GetMobileAxis(string axis)
        {
            if(axis == "Horizontal")
            {
                return GetTouch().deltaPosition.x;
            }
            else
            {
                return GetTouch().deltaPosition.y;
            }
        }

        public static float DistanceToPivot01(Vector3 fromPivot,Vector3 toPosition,float maxDelta)
        {
            float distance = Vector3.Distance(fromPivot, toPosition);

            if(distance > maxDelta)
            {
                return 1f;
            }
            else if(distance < 0f)
            {
                return 0f;
            }

            return distance / maxDelta;
        }

        public static float DistanceToPivot01(float fromValue, float toValue, float maxDelta)
        {
            return Mathf.Abs(fromValue - toValue) / maxDelta;
        }


        public static float GetDeltaSign(float pivot, float pos)
        {
            if (pivot < pos) return 1f;

            return -1f;
        }

        /// <summary>
        /// Gets mouse position in Editor.Gets touch position on mobile device.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetInput()
        {
#if UNITY_EDITOR
            return Input.mousePosition;
#elif UNITY_ANDROID || UNITY_IOS
            return GetTouch().position;
#endif
        }

        /// <summary>
        /// Returns true if the specifed key was pressed down in Editor.Returns true if finger touched the screen on mobile.
        /// </summary>
        /// <returns></returns>
        public static bool TouchBegan()
        {
#if UNITY_EDITOR
            return Input.GetMouseButtonDown(0);
#elif UNITY_ANDROID || UNITY_IOS
            if(!IsScreenTouched()) return false;

            return GetTouch().phase == TouchPhase.Began;
#endif
        }

        /// <summary>
        /// Returns true while specifed key is being held in Editor.Returns true while finger is idle on the screen on mobile.
        /// </summary>
        /// <returns></returns>
        public static bool TouchIdle()
        {
#if UNITY_EDITOR
            return Input.GetMouseButton(0);
#elif UNITY_ANDROID || UNITY_IOS
            if(!IsScreenTouched()) return false;

            return GetTouch().phase == TouchPhase.Stationary;
#endif

        }

        /// <summary>
        /// Returns true while specifed key is being held in Editor.Returns true while finger is moving on the screen on mobile.
        /// </summary>
        /// <returns></returns>
        public static bool TouchMoved()
        {
#if UNITY_EDITOR
            return Input.GetMouseButton(0);
#elif UNITY_ANDROID || UNITY_IOS
            if(!IsScreenTouched()) return false;

            return GetTouch().phase == TouchPhase.Moved;
#endif
        }

        /// <summary>
        /// Returns true if specifed key is held off in Editor.Returns true if finger is put off the screen on mobile.
        /// </summary>
        /// <returns></returns>
        public static bool TouchEnded()
        {
#if UNITY_EDITOR
            return Input.GetMouseButtonUp(0);
#elif UNITY_ANDROID || UNITY_IOS

            if(!IsScreenTouched()) return false;

            return GetTouch().phase == TouchPhase.Ended || GetTouch().phase == TouchPhase.Canceled;
#endif
        }
    }
    /// <summary>
    /// Use this for saving and loading the game.Look up Nimi Documentation for instructions.
    /// </summary>
    public static class NSaveLoadSystem
    {
        private static string path = Path.Combine(Application.persistentDataPath, "saves.json");

        /// <summary>
        /// Saves all data from a given object to default file.Look up Nimi Documentation for instructions.
        /// </summary>
        /// <param name="data"></param>
        public static void Save<T>(T data)
        {
            if(path == null)
            {
                Debug.LogError("Could not found the save path!");
                return;
            }

            string json = JsonUtility.ToJson(data);
            Debug.Log(path);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Saves all data from a given object to specified file.Look up Nimi Documentation for instructions.
        /// </summary>
        /// <param name="data"></param>
        public static void Save<T>(T data,string fileName)
        {
            string cpath = Path.Combine(Application.persistentDataPath, fileName, ".json");
            if (cpath == null)
            {
                Debug.LogError("Could not found the save path!");
                return;
            }

            string json = JsonUtility.ToJson(data);

            File.WriteAllText(cpath, json);
        }

        /// <summary>
        /// Loads all data of specified type from default file.Look up Nimi Documentation for instructions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Loaded data from default file</returns>
        public static T Load<T>()
        {
            if(path == null || !DefaultSavedDataExists())
            {
                Debug.LogError("Could not found the load path or data does not exists!");
                return default(T);
            }

            string json = File.ReadAllText(path);
            T data = JsonUtility.FromJson<T>(json);

            return data;
        }

        /// <summary>
        /// Loads all data of specified type from specified file.Look up Nimi Documentation for instructions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Loaded data from specified file</returns>
        public static T Load<T>(string fileName)
        {
            string cpath = Path.Combine(Application.persistentDataPath, fileName, ".json");
            if (cpath == null)
            {
                Debug.LogError("Could not find the load path or data does not exists!");
                return default(T);
            }

            if(!File.Exists(cpath))
            {
                Debug.LogError("Could not find the load path or data does not exists! Path: "+cpath);
                return default(T);
            }

            string json = File.ReadAllText(cpath);
            T data = JsonUtility.FromJson<T>(json);

            return data;
        }

        /// <summary>
        /// Is there any data saved? Use this to determine if game is first time started.
        /// </summary>
        /// <returns></returns>
        public static bool DefaultSavedDataExists()
        {
            return File.Exists(path);
        }


        public static void DeleteSavedData()
        {
            File.Delete(path);
        }

    }
    /// <summary>
    /// Wrapper for scene loading.
    /// </summary>
    public static class NLevelManager
    {
        private static int levelReached = 0;

        public static int Level
        {
            get
            {
                return levelReached;
            }
        }


        public static int sceneIndex
        {
            get
            {
                return SceneManager.GetActiveScene().buildIndex;
            }
        }

        /// <summary>
        /// Restarts current scene.
        /// </summary>
        public static void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        }

        /// <summary>
        /// Loads next scene,loads first scene if there are no more scenes.
        /// </summary>
        public static void LoadNextScene()
        {
            if(!ScenesExist())
            {
                Debug.LogError("Could not restart the scene because there are no scenes set in Build Settings.Go to File/BuildSettings and add scenes");
                return;
            }

            if(SceneManager.GetActiveScene().buildIndex + 1 >= SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }

        /// <summary>
        /// Loads scene with index specified in BuildSettings
        /// </summary>
        /// <param name="index"></param>
        public static void LoadSceneAt(int index)
        {
            SceneManager.LoadScene(index);
        }

        private static bool ScenesExist()
        {
            return SceneManager.sceneCountInBuildSettings > 0;
        }
    }

    /// <summary>
    /// Class for moving gameObjects around.
    /// </summary>
    public class NMover
    {

        /// <summary>
        /// Moves the gameObject to a position with given speed.
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="toPosition"></param>
        /// <param name="Speed"></param>
        public static void Move(Transform Object , Vector3 toPosition , float Speed)
        {
            NimiManager.instance.StartCoroutine(MoveCo(Object, toPosition, Speed));
        }

        private static IEnumerator MoveCo(Transform Object , Vector3 toPosition , float Speed)
        {
            while(Object.position != toPosition)
            {
                Object.position = Vector3.MoveTowards(Object.position, toPosition, Speed * Time.deltaTime);
                yield return 0;
            }

            Object.position = toPosition;
        }

        /// <summary>
        /// Moves the gameObject back and forth with given speed.
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="Speed"></param>
        public static void PingPong(Transform Object , Vector3 start , Vector3 end , float Speed)
        {
            NimiManager.instance.StartCoroutine(PingPongCo(Object, start, end, Speed));
        }

        private static IEnumerator PingPongCo(Transform Object , Vector3 start , Vector3 end , float Speed)
        {
            Object.position = start;

            loop:
            while (Object.position != end)
            {
                Object.position = Vector3.MoveTowards(Object.position, end, Speed * Time.deltaTime);
                yield return 0;
            }
            Object.position = end;
            Vector3 temp = start;
            start = end;
            end = temp;
            goto loop;
        }

        /// <summary>
        /// Scales the gameObject on all axis with given speed.
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="toScale"></param>
        /// <param name="Speed"></param>
        public static void Scale(Transform Object,Vector3 toScale, float Speed)
        {
            NimiManager.instance.StartCoroutine(ScaleCo(Object, toScale, Speed));
        }

        private static IEnumerator ScaleCo(Transform Object, Vector3 toScale, float Speed)
        {
            Vector3 s = Vector3.zero;
            while (NFloatUtility.CompareFloat(Object.localScale.x, 0f))
            {
                Object.localScale = Vector3.MoveTowards(Object.localScale, toScale, Time.deltaTime * Speed);
                yield return 0;
            }

            Object.localScale = toScale;
        }


        public static void RotateTowrads(Transform Object, Quaternion Rotation , float Duration)
        {
            NimiManager.instance.StartCoroutine(RotateCO(Object, Rotation, Duration));
        }


        private static IEnumerator RotateCO(Transform Object , Quaternion Rotation, float Duration)
        {
            float TimeStarted = Time.time;
            float TimeSinceStarted = Time.time - TimeStarted;
            float percentage = 0f;

            while(percentage < 1f)
            {
                TimeSinceStarted = Time.time - TimeStarted;
                percentage = TimeStarted / Duration;
                Object.rotation = Quaternion.Slerp(Object.rotation, Rotation, percentage);
                yield return 0;
            }
        }

        public static void MoveAndRotate(Transform Object, Vector3 Position, Quaternion Rotation, float Duration)
        {
            NimiManager.instance.StartCoroutine(MoveAndRotateCO(Object, Position, Rotation, Duration));
        }

        private static IEnumerator MoveAndRotateCO(Transform Object, Vector3 Position, Quaternion Rotation, float Duration)
        {
            float TimeStarted = Time.time;
            float TimeSinceStarted = Time.time - TimeStarted;
            float percentage = 0f;

            while (percentage < 1f)
            {
                TimeSinceStarted = Time.time - TimeStarted;
                percentage = TimeStarted / (Duration * 10f);
                Object.position = Vector3.Lerp(Object.position, Position, percentage);
                Object.rotation = Quaternion.Lerp(Object.rotation, Rotation, percentage);
                yield return 0;
            }
        }

        public static void Rotate(Transform Object, float Speed,Space space)
        {
            Object.Rotate(Vector3.up * Speed, space);
        }
    }

    /// <summary>
    /// Wrapper for managing and saving score.
    /// </summary>
    public class NScoreManager
    {
        private int Score;
        private int BestScore;

        public int currentScore
        {
            get
            {
                return Score;
            }
            set
            {
                Score = value;
            }
        }

        public int bestScore
        {
            get
            {
                return bestScore;
            }
            set
            {
                bestScore = value;
            }
        }


        /// <summary>
        /// Increases the score by given amount.
        /// </summary>
        /// <param name="amount"></param>
        public void IncreaseScore(int amount)
        {
            Score += amount;

            if (Score > bestScore) bestScore = Score;
        }

        /// <summary>
        /// Resets the score to 0;
        /// </summary>
        public void ResetScore()
        {
            Score = 0;
        }

        /// <summary>
        /// Saves the score to file.
        /// </summary>
        /// <param name="instance"></param>
        public static void Save(NScoreManager instance)
        {
            NSaveLoadSystem.Save(instance,"scoreSave");
        }

        /// <summary>
        /// Loads the score from file;
        /// </summary>
        /// <returns></returns>
        public static NScoreManager Load()
        {
            var saved = NSaveLoadSystem.Load<NScoreManager>("scoreSave");
            return saved;
        }

    }
    /// <summary>
    /// Class for comparing floats without precision problems.
    /// </summary>
    public class NFloatUtility 
    {
        /// <summary>
        /// Compares 2 floats with some tolerance.Use this instead of "==" when comparing floats.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="Tolerance"></param>
        /// <returns></returns>
        public static bool CompareFloat(float A, float B , float Tolerance)
        {
            return A >= B - Tolerance && A <= B + Tolerance;
        }

        /// <summary>
        /// Compares 2 floats with Epislon(default) tolerance.Use this instead of "==" when comparing floats.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool CompareFloat(float A, float B)
        {
            return A >= B - Mathf.Epsilon && A <= B + Mathf.Epsilon;
        }
    }

    public class NVector3Utility
    {
        public static Vector3 Dot(Vector3 A, Vector3 B)
        {
            A.x *= B.x;
            A.y *= B.y;
            A.z *= B.z;
            return A;
        }

        /// <summary>
        /// Returns distance between 2 vector points.Same as Vector3.Distance but without 2 squareroot calculations.Use this if you need distance calculation often.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static float Distance(Vector3 A, Vector3 B)
        {
            return (B - A).magnitude;
        }

        /// <summary>
        /// Returns distance between 2 vector points as Vector.Same as Vector3.Distance but without 2 squareroot calculations.Use this if you need distance calculation often.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Vector3 DistanceVector(Vector3 A, Vector3 B)
        {
            float dist = (B - A).sqrMagnitude;
            return Vector3.one * dist;
        }
    }

    public class NGameObjectUtility
    {
        private static Vector3 direction;
        private static Quaternion rotation;

        /// <summary>
        /// Returns the rotation thats pointing from one object to another object.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Quaternion GetRotationTowards(Vector3 from, Vector3 to)
        {
            direction = to - from;
            return Quaternion.LookRotation(direction);            
        }

        /// <summary>
        /// Returns the rotation towards direction.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Quaternion GetRotationTowardsDirection(Vector3 dir)
        {
            return Quaternion.LookRotation(dir);
        }

        public static bool CompareLayers(int layer, string other)
        {
            return layer == LayerMask.NameToLayer(other);
        }

        /// <summary>
        /// Returns the rotation thats pointing from one object to another object with smoothing.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Quaternion GetRotationTowards(Quaternion rotation, Vector3 from, Vector3 to,float lerp)
        {
            direction = to - from;
            return Quaternion.Lerp(rotation, Quaternion.LookRotation(direction), lerp);
        }

        public static void SetRagdoll(bool active, List<Transform> hiearchy)
        {
            foreach (Transform child in hiearchy)
            {
                Rigidbody rb = child.GetComponent<Rigidbody>();

                if(rb != null)
                {
                    rb.isKinematic = !active;
                    rb.useGravity = active;
                }
            }
        }

        public static void SetRagdoll(bool active,Transform parent)
        {
            List<Transform> hiearchy = GetHierarchyOfChildren(parent);

            foreach(Transform child in hiearchy)
            {
                Rigidbody rb = child.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.isKinematic = !active;
                    rb.useGravity = active;
                }
            }
        }

        public static Transform GetRootParent(Transform child)
        {
            Transform temp = child;

            while (true)
            {
                if(temp.parent == null)
                {
                    return temp;
                }
                temp = temp.parent;
            }
        }

        public static List<Transform> GetHierarchyOfChildren(Transform parent)
        {
            List<Transform> ret = new List<Transform>();

            AddChildrenOf(parent, ref ret);

            return ret;
        }

        private static void AddChildrenOf(Transform parent, ref List<Transform> ret)
        {
            for(int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                ret.Add(child);
                AddChildrenOf(child, ref ret);
            }
        }
       

        /// <summary>
        /// Gets all children in hierarchy in scene.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static List<GameObject> GetChildren(Transform parent)
        {
            List<GameObject> ret = new List<GameObject>();

            for(int i = 0; i < parent.childCount; i++)
            {
                ret.Add(parent.GetChild(i).gameObject);
            }

            return ret;
        }

        /// <summary>
        /// Gets all children in hierarchy in scene staring from index.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static List<GameObject> GetChildren(Transform parent, int beginIndex)
        {
            List<GameObject> ret = new List<GameObject>();

            for (int i = beginIndex; i < parent.childCount; i++)
            {
                ret.Add(parent.GetChild(i).gameObject);
            }

            return ret;
        }
    }

    /// <summary>
    /// Wrapper for asynchronus tasks.Runs coroutines internally.
    /// </summary>
    public class AsyncTask
    {
        private UnityAction Task;

        public AsyncTask(UnityAction task)
        {
            Task = task;
        }
        /// <summary>
        /// Runs the task after some time.
        /// </summary
        public void Run(float after)
        {
            NimiManager.instance.StartCoroutine(RunTask(after));
        }

        private IEnumerator RunTask(float after)
        {
            yield return new WaitForSeconds(after);
            Task.Invoke();
        }
    }

    /// <summary>
    /// Frequently used joystick for mobile games.Use this if you need rotation control.
    /// </summary>
    public class NJoystickInfinity
    {
        public float angle { get; set; }
        public Vector3 direction { get; set; }
        public Vector3 pivot { get; set; }
        public Quaternion calculatedRotation { get { return Quaternion.Euler(EulerRot); } }

        private Vector3 temp;
        private Vector3 EulerRot;
        private Vector3 Force;

        /// <summary>
        /// Initializes the joystick.
        /// </summary>
        public NJoystickInfinity()
        {
            temp = Vector3.zero;
            EulerRot = Vector3.zero;
        }

        /// <summary>
        /// Call this when player touches the screen and pass the touch position.
        /// </summary>
        /// <param name="position"></param>
        public void SetScreenPivot(Vector3 position)
        {
            pivot = position;
        }

        /// <summary>
        /// Call this when player touches the screen.
        /// </summary>
        public void SetScreenPivot()
        {
            pivot = NInput.GetInput();
        }

        /// <summary>
        /// Call this when you want the rotation to change(TouchMoved,TouchStationary etc) and pass the touch position;
        /// </summary>
        /// <param name="input"></param>
        public void Update(Vector3 input)
        {
            temp = input - pivot;
            direction = Vector3.ClampMagnitude(temp, 1f);
            angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            EulerRot.y = angle;
        }

        /// <summary>
        /// Call this when you want the rotation to change(TouchMoved,TouchStationary etc).
        /// </summary>
        public void Update()
        {
            temp = NInput.GetInput() - pivot;
            direction = Vector3.ClampMagnitude(temp, 1f);
            angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            EulerRot.y = angle;
        }
    }

    public class NVirtualJoystick
    {
        private Vector3 Offset;
        private Vector3 Point;
        private Vector3 Pivot;

        /// <summary>
        /// Initializes the joystick.
        /// </summary>
        public NVirtualJoystick()
        {
            Offset = Vector3.zero;
            Point = Vector3.zero;
            Pivot = Vector3.zero;
        }

        public void SetPivot()
        {
            Pivot = NInput.GetInput();
        }

        public void SetOffset(Vector3 position)
        {
            Offset = position - NInput.GetInput();
        }

        public void Update()
        {
            Point = NInput.GetInput() + Offset;
        }

        public Vector3 GetPoint()
        {
            return Point;
        }

        public Vector3 GetOffset()
        {
            return Offset;
        }

        public float GetMagnitude()
        {
            return Mathf.Abs(Point.x - Pivot.x) * GetSign();
        }

        public float GetSign()
        {
            return Point.x > Pivot.x ? 1f : -1f;
        }
    }

    public enum PointJoystickType
    {
        XYZ,XZ
    }

    /// <summary>
    /// Frequently used joystick for mobile games.Use this if you need direct point control.
    /// </summary>
    public class NPointJoystick
    {
        public Vector3 point { get { return Point; } set { Point = value; } }
        public Vector3 offset { get; set; }
        public LayerMask layer { get; set;}
        public bool useOffset { get; set; }
        public float defaultYPoint { get; set; }
        public float distortion { get; set; }

        private Vector3 Point;
        private Ray ray;
        private RaycastHit hit;
        private Camera cam;
        private PointJoystickType type;

        /// <summary>
        /// Initializes the joystick.Pass the layer your detection plane uses.Look up Nimi Documentation for insturctions.
        /// </summary>
        /// <param name="mask"></param>
        public NPointJoystick(LayerMask mask,PointJoystickType type)
        {
            cam = Camera.main;
            layer = mask;
            offset = Vector3.zero;
            Point = Vector3.zero;
            defaultYPoint = 0f;
            distortion = 0f;
            this.type = type;
        }

        /// <summary>
        /// Call this when player touches the screen if you have useOffset enabled.Pass the touchPosition and objectPostion.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="objectPosition"></param>
        public void SetOffset(Vector3 input,Vector3 objectPosition)
        {
            if (!useOffset) return;

            ray = cam.ScreenPointToRay(input);

            if (Physics.Raycast(ray, out hit, 1000f, layer))
            {
                offset = objectPosition - hit.point;
            }
        }

        /// <summary>
        /// Call this when player touches the screen if you have useOffset enabled.Pass the objectPostion.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="objectPosition"></param>
        public void SetOffset(Vector3 objectPosition)
        {
            Vector3 input = NInput.GetInput();
            if (!useOffset) return;

            ray = cam.ScreenPointToRay(input);

            if (Physics.Raycast(ray, out hit, 1000f, layer))
            {
                offset = objectPosition - hit.point;
            }
        }

        /// <summary>
        /// Call this when you want to update the position(TouchMoved,TouchStationary etc.).Pass the touch position.
        /// </summary>
        /// <param name="input"></param>
        public void Update(Vector3 input)
        {
            ray = cam.ScreenPointToRay(input);

            if (Physics.Raycast(ray, out hit, 1000f, layer))
            {
                Point = useOffset ? hit.point + offset : hit.point + Random.insideUnitSphere*distortion;
                if(type == PointJoystickType.XZ)
                {
                    Point.y = NFloatUtility.CompareFloat(defaultYPoint, 0f) ? defaultYPoint : Point.y;
                }
            }
            else
            {
                Debug.LogWarning("Can't find plane to hit with layer " + layer);
            }
        }

        /// <summary>
        /// Call this when you want to update the position(TouchMoved,TouchStationary etc.).
        /// </summary>
        /// <param name="input"></param>
        public void Update()
        {
            Vector3 input = NInput.GetInput();

            ray = cam.ScreenPointToRay(input);

            if (Physics.Raycast(ray, out hit, 1000f, layer))
            {
                Point = useOffset ? hit.point + offset : hit.point + Random.insideUnitSphere * distortion;
                if (type == PointJoystickType.XZ)
                {
                    Point.y = NFloatUtility.CompareFloat(defaultYPoint, 0f) ? defaultYPoint : Point.y;
                }
            }
            else
            {
                Debug.LogWarning("Can't find plane to hit with layer " + layer);
            }
        }
    }

    public class NScreenUtility
    {
        private List<Vector2> Points;

        public NScreenUtility()
        {
            Points = new List<Vector2>();
        }

        public void ClearPoints()
        {
            Points.Clear();
        }

        public void AddScreenPoint(Vector2 Point)
        {
            Points.Add(Point);
        }

        public Vector2 GetPoint(int i)
        {
            return Points[i];
        }
    }


    public static class ObjectSwaper
    {
        public static void Swap(Transform parent)
        {
            if (parent.childCount == 0) return;

            parent.GetChild(0).gameObject.SetActive(false);

            for(int i = 1; i < parent.childCount; i++)
            {
                parent.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    
}



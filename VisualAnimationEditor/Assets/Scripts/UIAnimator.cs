using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace EasyUIAnimator {
    // UI Animator
    [ExecuteInEditMode] //使脚本的所有实例都在编辑模式下执行
    public class UIAnimator : MonoBehaviour {
        private static UIAnimator instance; //单例
        private List<UIAnimation> animations;//动画列表
        private List<UIAnimation> removeList;
        private Vector2 screenDimension;//屏幕尺寸
        private Vector2 invertedScreenDimension;//屏幕尺寸倒维数,x,y各取倒数
        private bool scaleWithScreen;//是否按屏幕缩放
        // private float canvasScale;
        private static UIAnimator Instance { get { return instance; } }
        public static Vector2 ScreenDimension { get { return instance.screenDimension; } }
        public static Vector2 InvertedScreenDimension { get { return instance.invertedScreenDimension; } }
        public static bool ScaleWithScreen { get { return instance.scaleWithScreen; } }
        public static List<UIAnimation> Animations { get { return instance.animations; } }

        #region UNITY
        void Awake() {
            if (instance == null) { //初始化数据
                instance = this;
                animations = new List<UIAnimation>();
                removeList = new List<UIAnimation>();
                try {
                    scaleWithScreen = FindObjectOfType<CanvasScaler>().uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    screenDimension = new Vector2(Screen.width, Screen.height);
                    invertedScreenDimension = new Vector2(1f / screenDimension.x, 1f / screenDimension.y);
                } catch (System.NullReferenceException) {
                    Debug.Log("Please, add a Canvas to your scene");
                }
            }
        }

        void Start() {
        }

        void Update() {
#if UNITY_EDITOR
            if (animations == null) {
                Awake();
            }
            if (!Application.isPlaying) {
                try {
                    screenDimension = new Vector2(Screen.width, Screen.height);
                    invertedScreenDimension = new Vector2(1f / screenDimension.x, 1f / screenDimension.y);
                } catch (System.NullReferenceException) {
                    Debug.Log("Please, add a Canvas to your scene");
                }
            }
#endif
            if (animations.Count > 0) {
                for (int i = 0; i < animations.Count; i++) {
                    if (!animations[i].Update(Time.deltaTime)) {
                        removeList.Add(animations[i]);
                    }
                }
            }
            RemoveSafely();
        }

        #endregion

        #region PUBLIC
        // Get Center
        // Used to get the anchor center of <transform>.
        public static Vector2 GetCenter(RectTransform transform) {
            return Vector2.Scale(transform.position, Instance.invertedScreenDimension);
        }

        // Add Animation
        // Animation uses to add itself to the Animator.
        public static UIAnimation AddAnimation(UIAnimation animation) {
            instance.animations.Add(animation);
            return animation;
        }

        // Remove Animation Removes animations from updating list.
        public static void RemoveAnimation(UIAnimation anim) {
            foreach (UIAnimation animation in instance.animations) {
                if (animation == anim) {
                    instance.removeList.Add(animation);
                    return;
                }
            }
        }

        #endregion

        #region PRIVATE
        // Remove Safely.
        // After finished, the animation cannot be removed
        // inside the iterator. It is store in another list
        // to be remove with safety.

        private void RemoveSafely() {
            if (removeList.Count > 0) {
                foreach (var item in removeList) {
                    animations.Remove(item);
                    if (item.OnFinish != null) {
                        item.OnFinish();
                    }
                }
                removeList.Clear();
            }
        }

        #endregion

        //位移动画
        #region MOVE_ANIMATION
        // Move*
        // Movement animations. 
        // It creates an animation to move <transform>
        // from <origin> to <target> in <duration>
        // seconds.
        // - MOVE TO: 				From (current position) to (Vector2 target).
        // - MOVE HORIZONTAL: 		From (float origin) to (float target) with fixed y.
        // - MOVE HORIZONTAL TO: 	From (current position) to (float target) with fixed y.
        // - MOVE HORIZONTAL: 		From (float origin) to (float target) with fixed x.
        // - MOVE HORIZONTAL TO: 	From (current position) to (float target) with fixed x.
        // - MOVE OFFSET: 			From (current position) to (current position + Vector2 offset).

        public static UIPositionAnimation Move(RectTransform transform, Vector2 origin, Vector2 target, float duration) {
            return new UIPositionAnimation(transform: transform, origin: origin, target: target, duration: duration);
        }

        public static UIPositionAnimation MoveTo(RectTransform transform, Vector2 target, float duration) {
            return Move(transform, GetCenter(transform), target, duration);
        }

        public static UIPositionAnimation MoveHorizontal(RectTransform transform, float origin, float target, float duration) {
            return Move(transform, new Vector2(origin, GetCenter(transform).y), new Vector2(target, GetCenter(transform).y), duration);
        }

        public static UIPositionAnimation MoveHorizontalTo(RectTransform transform, float target, float duration) {
            return Move(transform, GetCenter(transform), new Vector2(target, GetCenter(transform).y), duration);
        }

        public static UIPositionAnimation MoveVertical(RectTransform transform, float origin, float target, float duration) {
            return Move(transform, new Vector2(GetCenter(transform).x, origin), new Vector2(GetCenter(transform).x, target), duration);
        }

        public static UIPositionAnimation MoveVerticalTo(RectTransform transform, float target, float duration) {
            return Move(transform, GetCenter(transform), new Vector2(GetCenter(transform).x, target), duration);
        }

        public static UIPositionAnimation MoveOffset(RectTransform transform, Vector2 offset, float duration) {
            return Move(transform, GetCenter(transform), GetCenter(transform) + offset, duration);
        }

        public static UIPositionAnimation MoveBezier(RectTransform transform, Vector2 origin, Vector2 target, Vector2 p1, Vector2 p2, float duration) {
            return new UIBezierAnimation(
                transform: transform,
                origin: origin,
                target: target,
                p1: p1,
                p2: p2,
                duration: duration
            );
        }

        public static UIPositionAnimation MoveBezier(RectTransform transform, Vector2 origin, Vector2 target, Vector2 p1, float duration) {
            return new UIBezierAnimation(
                transform: transform,
                origin: origin,
                target: target,
                p1: p1,
                duration: duration
            );
        }

        #endregion

        //缩放动画
        #region SCALE_ANIMATION
        // Scale animations.
        // It creates an animation to scale <transform>
        // from <origin> to <target> in <duration>
        // seconds.
        // - SCALE TO: 			From (current scale) to (Vector3 target).
        // - SCALE OFFSET: 		From (current scale) to (current scale + Vector3 offset).

        public static UIScaleAnimation Scale(RectTransform transform, Vector3 origin, Vector3 target, float duration) {
            return new UIScaleAnimation(
                transform: transform,
                origin: origin,
                target: target,
                duration: duration
            );
        }

        public static UIScaleAnimation ScaleTo(RectTransform transform, Vector3 target, float duration) {
            return Scale(transform, transform.localScale, target, duration);
        }

        public static UIScaleAnimation ScaleOffset(RectTransform transform, Vector3 offset, float duration) {
            return Scale(transform, transform.localScale, transform.localScale + offset, duration);
        }

        #endregion

        //旋转动画
        #region ROTATION_ANIMATION
        // Rotation*
        // Rotation animations.
        // It creates an animation to rotate <transform>
        // from <origin> to <target> in <duration>
        // seconds.
        // - ROTATE TO: 		From (current rotation) to (Quaternion target).
        // - ROTATE OFFSET: 	From (current rotation) to (current rotation + Quaterion offset).


        public static UIRotationAnimation Rotate(RectTransform transform, Quaternion origin, Quaternion target, float duration) {
            return new UIRotationAnimation(
                transform: transform,
                origin: origin,
                target: target,
                duration: duration
            );
        }

        public static UIRotationAnimation RotateTo(RectTransform transform, Quaternion target, float duration) {
            return Rotate(transform, transform.localRotation, target, duration);
        }

        public static UIRotationAnimation RotateOffset(RectTransform transform, Quaternion offset, float duration) {
            return Rotate(transform, transform.localRotation, transform.localRotation * offset, duration);
        }

        // UNCLAMPED : THE ROTATION IS NOT LIMITED TO THE 360 DEGREES, BUT LIMITED TO THE Z AXIS
        public static UIRotationAnimation Rotate(RectTransform transform, float originAngle, float targetAngle, float duration) {
            return new UIRotationAnimation(
                transform: transform,
                origin: originAngle,
                target: targetAngle,
                duration: duration
            );
        }

        public static UIRotationAnimation RotateTo(RectTransform transform, float targetAngle, float duration) {
            return Rotate(transform, transform.localRotation.eulerAngles.z, targetAngle, duration);
        }

        public static UIRotationAnimation RotateOffset(RectTransform transform, float offsetAngle, float duration) {
            return Rotate(transform, transform.localRotation.eulerAngles.z, transform.localRotation.eulerAngles.z + offsetAngle, duration);
        }

        #endregion

        //Graphic颜色动画
        #region IMAGE_ANIMATION
        // ChangeColor*, Fade*
        // Image animations.
        // It creates an animation to change color 
        // of <image> from <originColor> to <targetColor>
        // in <duration> seconds.
        // - CHANGE COLOR TO: 		From (current color) to (Color targetColor).
        // - FADE IN:              Creates a fade in effect.
        // - FADE OUT:             Creates a fade out effect.
        public static UISpriteAnimation ChangeColor(Graphic image, Color originColor, Color targetColor, float duration) {
            return new UISpriteAnimation(
                image: image,
                originColor: originColor,
                targetColor: targetColor,
                duration: duration
            );
        }

        public static UISpriteAnimation ChangeColorTo(Graphic image, Color targetColor, float duration) {
            return ChangeColor(image, image.color, targetColor, duration);
        }

        public static UISpriteAnimation FadeIn(Graphic image, float duration) {
            Color originColor = image.color;
            originColor.a = 0;
            Color targetColor = image.color;
            targetColor.a = 1;
            return ChangeColor(image, originColor, targetColor, duration);
        }

        public static UISpriteAnimation FadeOut(Graphic image, float duration) {
            Color originColor = image.color;
            originColor.a = 1;
            Color targetColor = image.color;
            targetColor.a = 0;
            return ChangeColor(image, originColor, targetColor, duration);
        }

        #endregion
    }

    // UI Animation
    // 动画对象的抽象类,定义动画的行为和基础数据
    public abstract class UIAnimation {
        public delegate void AnimationCallback(); //动画播放完的回调函数
        private float timer = 0; // 运行的时间占用率
        private float delay = 0; // 延时
        private bool paused = false; //暂停
        private AnimationCallback onFinish; //动画完成的回调委托
        protected float duration; //持续时间
        protected UpdateBehaviour updateBehaviour; //更新的委托函数,返回float
                                                   //属性字段区域
        public float Duration { get { return duration; } }
        public float Delay { get { return delay; } }
        public AnimationCallback OnFinish { get { return onFinish; } }

        // Update 返回bool值代表运行是否可以继续执行,true 未完成可继续执行 false 完成,不可执行
        public bool Update(float deltaTime) {
            if (paused) {
                return true;
            }
            timer += deltaTime / duration;
            //Debug.Log("timer: " + timer);
            if (timer < 0) { //小于0未开始
                OnUpdate(0);
            } else if (timer < 1) { //0 - 1的时间占用率 表示正在运行
                OnUpdate(timer);
            } else { //运行结束,调用OnEnd
                OnEnd();
                return false;
            }
            return true;
        }

        public abstract void OnUpdate(float timer); //OnUpdate抽象方法,由每个动画行为具体实现
        public abstract void OnEnd(); // 动画结束之后调用
        public abstract void Reverse(); //反转动画,ping-pong

        // 为动画设置效果,返回当前动画对象
        public virtual UIAnimation SetEffect(Effect.EffectUpdate effect, Quaternion rotation = default(Quaternion)) {
            return this;
        }

        // 设置动画执行完成的回调,callback 执行的委托,add 设置模式,默认false:添加 true:替换
        public UIAnimation SetCallback(AnimationCallback callback, bool add = false) {
            if (add) { //添加
                onFinish += callback;
            } else { //替换
                onFinish = callback;
            }
            return this;
        }

        // 设置修改器,修改器改变计时器如何影响动画,传入修改器委托
        public UIAnimation SetModifier(UpdateBehaviour updateBehaviour) {
            this.updateBehaviour = updateBehaviour; //动画更新的表现
            return this;
        }

        // 延时
        public UIAnimation SetDelay(float delay) {
            this.delay = delay;
            timer = -delay / duration; //将时间占用率回调
            return this;
        }

        // 循环播放,传入是否pingPong的动画表现
        public UIAnimation SetLoop(bool pingPong = false) {
            SetCallback(() => { //设置播放完成的回调
                if (pingPong) { //若是pingpong就设置为翻转的动画
                    Reverse();
                }
                Play();//播放
            });
            return this;
        }

        // 播放
        public void Play() {
            if (paused) { //若是暂停将pause标识置为false
                paused = false;
            } else {
                Restart(); //重新开始
            }
        }

        // 暂停 playIfPaused若是暂停状态则恢复播放,默认false
        public void Pause(bool playIfPaused = false) {
            if (playIfPaused) { //暂停状态恢复播放
                if (paused) { // 暂停中
                    Play(); //恢复播放
                } else {
                    paused = true; //暂停
                }
            } else {
                paused = true; //暂停
            }
        }

        // 重新开始动画
        public void Restart() {
            SetDelay(delay); //设置延时
            if (!UIAnimator.Animations.Contains(this)) { //当前播放的动画列表不包含自身
                UIAnimator.AddAnimation(this);//添加到动画列表
            }
        }

        // 停止动画
        public void Stop() {
            UIAnimator.RemoveAnimation(this); //动画管理器将自身移除
        }
    }

    // 位移动画
    public class UIPositionAnimation : UIAnimation {
        protected RectTransform transform;
        private Vector2 originPosition;
        private Vector2 targetPosition;
        protected Effect.EffectBehaviour effectBehaviour;

        public UIPositionAnimation(RectTransform transform, UIPositionAnimation animation) : this(transform, animation.originPosition, animation.targetPosition, animation.duration) {
            originPosition = animation.originPosition;
            targetPosition = animation.targetPosition;
            updateBehaviour = animation.updateBehaviour;
            effectBehaviour = animation.effectBehaviour;
        }

        public UIPositionAnimation(RectTransform transform, Vector2 origin, Vector2 target, float duration) {
            Canvas canvas = UIAnimator.FindObjectOfType<Canvas>();
            float canvasScale = (UIAnimator.ScaleWithScreen) ? 1 / canvas.scaleFactor : 1;
            this.transform = transform;
            this.duration = duration < 0.0000001f ? 0.0000001f : duration;
            Vector2 position = (canvas.renderMode == RenderMode.ScreenSpaceCamera) ? canvas.worldCamera.WorldToScreenPoint(transform.position) : transform.position;
            this.originPosition = Vector2.Scale(origin, UIAnimator.ScreenDimension) * canvasScale - position * canvasScale + transform.anchoredPosition;
            this.targetPosition = Vector2.Scale(target, UIAnimator.ScreenDimension) * canvasScale - position * canvasScale + transform.anchoredPosition;
            updateBehaviour = Modifier.Linear;
            effectBehaviour = Effect.NoEffect;
            // DEBUG
            // Debug.Log("Screen Dimention: " + UIAnimator.ScreenDimension);
            // Debug.Log("Canvas Scale: " + canvasScale);
            // Debug.Log("Origin: " + this.originPosition + " (" + Vector2.Scale(origin, UIAnimator.ScreenDimension) + " - " + position + " + " + transform.anchoredPosition + ")");
            // Debug.Log("Target: " + this.targetPosition + " (" + Vector2.Scale(target, UIAnimator.ScreenDimension) + " - " + position + " + " + transform.anchoredPosition + ")");
        }

        public override void OnUpdate(float timer) {
            transform.anchoredPosition = Vector2.Lerp(originPosition, targetPosition, updateBehaviour(timer)) + effectBehaviour(timer);
        }

        public override void OnEnd() {
            transform.anchoredPosition = targetPosition;
        }

        public override void Reverse() {
            Vector3 aux = originPosition;
            originPosition = targetPosition;
            targetPosition = aux;
        }

        // Set Effect
        // For more on Effects, please see Effects class
        public override UIAnimation SetEffect(Effect.EffectUpdate effect, Quaternion rotation = default(Quaternion)) {
            Vector2 direction = (targetPosition - originPosition).normalized;
            direction = (direction == Vector2.zero) ? Vector2.right : direction;
            Vector2 directionVector = rotation * direction;
            directionVector *= UIAnimator.ScreenDimension.y;
            this.effectBehaviour = Effect.GetBehaviour(effect, directionVector);
            return this;
        }
    }

    //贝塞尔动画
    public class UIBezierAnimation : UIPositionAnimation {
        public UIBezierAnimation(RectTransform transform, Vector2 origin, Vector2 target, float duration, Vector2 p1, Vector2 p2) : base(transform, origin, target, duration) {
            float canvasScale = (UIAnimator.ScaleWithScreen) ? 1 / UIAnimator.FindObjectOfType<Canvas>().scaleFactor : 1;
            Vector2 mP0 = Vector2.Scale(origin, UIAnimator.ScreenDimension) * canvasScale - (Vector2)transform.position * canvasScale + transform.anchoredPosition;
            Vector2 mP1 = Vector2.Scale(p1, UIAnimator.ScreenDimension) * canvasScale - (Vector2)transform.position * canvasScale + transform.anchoredPosition;
            Vector2 mP2 = Vector2.Scale(p2, UIAnimator.ScreenDimension) * canvasScale - (Vector2)transform.position * canvasScale + transform.anchoredPosition;
            Vector2 mP3 = Vector2.Scale(target, UIAnimator.ScreenDimension) * canvasScale - (Vector2)transform.position * canvasScale + transform.anchoredPosition;
            effectBehaviour = Effect.BezierEffectBehaviour(mP0, mP1, mP2, mP3);
        }

        public UIBezierAnimation(RectTransform transform, Vector2 origin, Vector2 target, float duration, Vector2 p1) : base(transform, origin, target, duration) {
            float canvasScale = (UIAnimator.ScaleWithScreen) ? UIAnimator.FindObjectOfType<Canvas>().scaleFactor : 1;
            Vector2 mP0 = Vector2.Scale(origin, UIAnimator.ScreenDimension) - (Vector2)transform.position / canvasScale + transform.anchoredPosition;
            Vector2 mP1 = Vector2.Scale(p1, UIAnimator.ScreenDimension) - (Vector2)transform.position / canvasScale + transform.anchoredPosition;
            Vector2 mP2 = Vector2.Scale(target, UIAnimator.ScreenDimension) - (Vector2)transform.position / canvasScale + transform.anchoredPosition;
            effectBehaviour = Effect.BezierEffectBehaviour(mP0, mP1, mP2);
        }

        public override void OnUpdate(float timer) {
            transform.anchoredPosition = effectBehaviour(updateBehaviour(timer));
        }
    }

    // UI Scale Animation
    // UI Animation - Scale Animation
    // Overrides superclass abstract methods.
    // Updates transform localScale.
    // 缩放动画 
    public class UIScaleAnimation : UIAnimation {
        private RectTransform transform;
        private Vector3 originScale;
        private Vector3 targetScale;
        private Effect.EffectBehaviour effectBehaviour;

        public UIScaleAnimation(RectTransform transform, UIScaleAnimation animation) :
        this(transform, animation.originScale, animation.targetScale, animation.duration) { }

        public UIScaleAnimation(RectTransform transform, Vector3 origin, Vector3 target, float duration) {
            this.transform = transform;
            this.duration = duration < 0.0000001f ? 0.0000001f : duration;
            this.originScale = origin;
            this.targetScale = target;
            updateBehaviour = Modifier.Linear;
            effectBehaviour = Effect.NoEffect;
        }

        public override void OnUpdate(float timer) {
            transform.localScale = Vector3.Lerp(originScale, targetScale, updateBehaviour(timer)) + (Vector3)effectBehaviour(timer);
        }

        public override void OnEnd() {
            transform.localScale = targetScale;
        }

        public override void Reverse() {
            Vector3 aux = originScale;
            originScale = targetScale;
            targetScale = aux;
        }

        // Set Effect
        // For more on Effects, please see Effects class

        public override UIAnimation SetEffect(Effect.EffectUpdate effect, Quaternion rotation = default(Quaternion)) {
            this.effectBehaviour = Effect.GetBehaviour(effect, rotation * (targetScale - originScale));
            return this;
        }
    }

    // UI Rotation Animation
    // UI Animation - Rotation Animation
    // Overrides superclass abstract methods.
    // Updates transform localRotation.
    // 旋转动画
    public class UIRotationAnimation : UIAnimation {
        private RectTransform transform;
        private float originAngle;
        private float targetAngle;
        private Quaternion originRotation;
        private Quaternion targetRotation;
        private Effect.EffectBehaviour effectBehaviour;
        private bool unclamped = true;

        public UIRotationAnimation(RectTransform transform, UIRotationAnimation animation) : this(transform, animation.originAngle, animation.targetAngle, animation.duration) {

        }

        public UIRotationAnimation(RectTransform transform, Quaternion origin, Quaternion target, float duration) {
            this.transform = transform;
            this.duration = duration < 0.0000001f ? 0.0000001f : duration;
            this.originRotation = origin;
            this.targetRotation = target;
            updateBehaviour = Modifier.Linear;
            effectBehaviour = Effect.NoEffect;
            unclamped = false;
        }

        public UIRotationAnimation(RectTransform transform, float origin, float target, float duration) {
            this.transform = transform;
            this.duration = duration < 0.0000001f ? 0.0000001f : duration;
            this.originAngle = origin;
            this.targetAngle = target;
            updateBehaviour = Modifier.Linear;
            effectBehaviour = Effect.NoEffect;
            unclamped = true;
        }

        public override void OnUpdate(float timer) {
            if (unclamped) {
                transform.localRotation = Quaternion.AngleAxis(Mathf.Lerp(originAngle, targetAngle, timer), Vector3.forward) * Quaternion.Euler(Vector3.forward * effectBehaviour(timer).x);
            } else {
                transform.localRotation = Quaternion.Lerp(originRotation, targetRotation, updateBehaviour(timer)) * Quaternion.Euler(Vector3.forward * effectBehaviour(timer).x);
            }
        }

        public override void OnEnd() {
            if (unclamped) {
                transform.localRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
            } else {
                transform.localRotation = targetRotation;
            }
        }

        public override void Reverse() {
            if (unclamped) {
                float aux = originAngle;
                originAngle = targetAngle;
                targetAngle = aux;
            } else {
                Quaternion aux = originRotation;
                originRotation = targetRotation;
                targetRotation = aux;
            }
        }

        public override UIAnimation SetEffect(Effect.EffectUpdate effect, Quaternion rotation = default(Quaternion)) {
            this.effectBehaviour = Effect.GetBehaviour(effect, Vector2.right);
            return this;
        }
    }

    // Graphic组件的颜色动画
    public class UISpriteAnimation : UIAnimation {
        private Graphic image;
        private Color originColor;
        private Color targetColor;

        public UISpriteAnimation(Graphic img, UISpriteAnimation animation) :
        this(img, animation.originColor, animation.targetColor, animation.duration) { }

        public UISpriteAnimation(Graphic image, Color originColor, Color targetColor, float duration) {
            this.image = image;
            this.duration = duration;
            this.originColor = originColor;
            this.targetColor = targetColor;
            updateBehaviour = Modifier.Linear;
        }

        public override void OnUpdate(float timer) {
            image.color = Color.Lerp(originColor, targetColor, updateBehaviour(timer));
        }

        public override void OnEnd() {
            image.color = targetColor;
        }

        public override void Reverse() {
            Color aux = originColor;
            originColor = targetColor;
            targetColor = aux;
        }
    }

    // UI Group Animation
    // UI Animation - Group Animation
    // Overrides superclass abstract methods.
    // Updates multiple UI Animation. Used as reusable animation.

    //对象组动画
    public class UIGroupAnimation : UIAnimation {
        private UIAnimation[] animations;
        private bool[] finished;
        private float lastTime;

        public UIGroupAnimation(RectTransform[] rects, UIPositionAnimation transformAnimation) {
            animations = new UIAnimation[rects.Length];
            for (int i = 0; i < animations.Length; i++) {
                animations[i] = new UIPositionAnimation(rects[i], transformAnimation);
            }
            duration = animations[0].Duration;
            finished = new bool[animations.Length];
        }

        public UIGroupAnimation(RectTransform[] rects, UIScaleAnimation transformAnimation) {
            animations = new UIAnimation[rects.Length];
            for (int i = 0; i < animations.Length; i++) {
                animations[i] = new UIScaleAnimation(rects[i], transformAnimation);
            }
            duration = animations[0].Duration;
            finished = new bool[animations.Length];
        }

        public UIGroupAnimation(Image[] imgs, UISpriteAnimation spriteAnimation) {
            animations = new UIAnimation[imgs.Length];
            for (int i = 0; i < animations.Length; i++) {
                animations[i] = new UISpriteAnimation(imgs[i], spriteAnimation);
            }
            duration = animations[0].Duration;
            finished = new bool[animations.Length];
        }

        public UIGroupAnimation(params UIAnimation[] animations) {
            for (int i = 0; i < animations.Length; i++) {
                duration = Mathf.Max(duration, animations[i].Duration + animations[i].Delay);
            }
            this.animations = animations;
            finished = new bool[animations.Length];
        }

        public override void OnUpdate(float timer) {
            float deltaTime = (timer - lastTime) * duration;
            for (int i = 0; i < animations.Length; i++) {
                if (!finished[i]) {
                    finished[i] = !animations[i].Update(deltaTime);
                }

            }
            lastTime = timer;
        }

        public override void OnEnd() {
            for (int i = 0; i < animations.Length; i++) {
                animations[i].OnEnd();
                finished[i] = false;
                animations[i].SetDelay(animations[i].Delay);
                lastTime = 0;
            }
        }

        public override void Reverse() {
            for (int i = 0; i < animations.Length; i++) {
                animations[i].Reverse();
            }
        }

        public override UIAnimation SetEffect(Effect.EffectUpdate effect, Quaternion rotation = default(Quaternion)) {
            for (int i = 0; i < animations.Length; i++) {
                animations[i].SetEffect(effect, rotation);
            }
            return this;
        }

        // Set Group Modifier
        // Set same modifier for all animations
        public UIGroupAnimation SetGroupModifier(UpdateBehaviour mod) {
            for (int i = 0; i < animations.Length; i++) {
                animations[i].SetModifier(mod);
            }
            return this;
        }

        // Set Group Effect
        // Set same effect for all animations
        public UIGroupAnimation SetGroupEffect(Effect.EffectGroup effectGroup, float max = 0.2f, float min = 0.0f, int maxBounce = 2, int minBounce = 1, int minAngle = 0, int maxAngle = 0) {
            for (int i = 0; i < animations.Length; i++) {
                animations[i].SetEffect(effectGroup(Random.Range(min, max), Random.Range(minBounce, maxBounce)), Quaternion.Euler(Vector3.forward * Random.Range(minAngle, maxAngle)));
            }
            return this;
        }
    }

    public delegate float UpdateBehaviour(float deltaTime);
    // Modifier
    // Change animation behaviour. 
    // Returns a float value used in inside
    // UIAnimation.OnUpdate to change the timer
    // growth curve, changing the animation.
    // To add a new modifier simply create a new
    // UpdateBehaviour function.
    // CAUTION: 
    // 1. Functions must attend: f(0) = 0 & f(1) = 1.
    // 2. It is used inside a Lerp function, any 
    // values above 1 may have unexpected behaviour.
    public static class Modifier {
        public static float Linear(float time) { return time; }
        public static float QuadOut(float time) { return time * time; }
        public static float QuadIn(float time) { return (float)Mathf.Pow(time, 0.50f); }
        public static float CubOut(float time) { return time * time * time; }
        public static float CubIn(float time) { return Mathf.Pow(time, 0.33f); }
        public static float PolyOut(float time) { return time * time * time * time; }
        public static float PolyIn(float time) { return Mathf.Pow(time, 0.25f); }
        public static float Sin(float time) { return 0.5f + 0.5f * Mathf.Cos((1 - time) * Mathf.PI); }
        public static float Tan(float time) { return 2 * time - Sin(time); }
        public static float CircularIn(float time) { return Mathf.Sqrt(Mathf.Sin(time * Mathf.PI / 2)); }
        public static float CircularOut(float time) { return 1 - Mathf.Sqrt(Mathf.Cos(-time * Mathf.PI / 2)); }
    }

    // Effect
    // Add new values to the animation. 
    // Returns a Vector2 from (float time) adding a new behaviour
    // to the animation.
    // To add a new effect you must create a new EffectUpdate function
    // You can use a float and a int parameter to adjust your effect
    // CAUTION:
    // 1. Functions must attend: f(0) = 0 & f(1) = 0.
    // 2. You must also create a EffectGroup, so the effect can be
    // used in a GroupAnimation
    public static class Effect {
        public delegate EffectUpdate EffectGroup(float max, int bounce);
        public delegate Vector2 EffectBehaviour(float time);
        public delegate float EffectUpdate(float time);
        public static Vector2 NoEffect(float time) { return Vector2.zero; }

        public static EffectUpdate Spring(float max = 0.2f, int bounce = 2) { return (float time) => { return max * (1 - time * time) * Mathf.Sin(Mathf.PI * bounce * time * time); }; }
        public static EffectUpdate Wave(float max = 0.2f, int bounce = 2) { return (float time) => { return max * Mathf.Sin(Mathf.PI * bounce * time); }; }
        public static EffectUpdate Explosion(float max = 0.2f) { return (float time) => { return max * Mathf.Sqrt(Mathf.Sin(Mathf.Pow(time, 0.75f) * Mathf.PI)); }; }

        public static EffectGroup SpringGroup() { return (float max, int bounce) => { return Spring(max, bounce); }; }
        public static EffectGroup WaveGroup() { return (float max, int bounce) => { return Wave(max, bounce); }; }
        public static EffectGroup ExplosionGroup() { return (float max, int bounce) => { return Explosion(max); }; }

        // Get Behaviour
        // NOTE: For movement animations, changing the directionVector can
        // modify you effect. 
        public static EffectBehaviour GetBehaviour(EffectUpdate update, Vector2 directionVector) {
            return ((float time) => { return directionVector * update(time); });
        }
        public static EffectBehaviour BezierEffectBehaviour(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {
            return (float time) => { return (1 - time) * (1 - time) * (1 - time) * p0 + 3 * (1 - time) * (1 - time) * time * p1 + 3 * (1 - time) * time * time * p2 + time * time * time * p3; };
        }
        public static EffectBehaviour BezierEffectBehaviour(Vector2 p0, Vector2 p1, Vector2 p2) {
            return (float time) => { return (1 - time) * (1 - time) * p0 + 2 * (1 - time) * time * p1 + time * time * p2; };
        }
    }
}
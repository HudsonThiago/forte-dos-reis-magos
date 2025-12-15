using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Entities
{
    public class AnimationSystem : MonoBehaviour
    {
        public Animator animator;
        public string currentAnimation = "";
        List<AnimationClip> animationClipList;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animationClipList = animator.runtimeAnimatorController.animationClips.ToList();
        }
        
        public float changeAnimation(string animation, float crossFade = 0)
        {
            if (currentAnimation != animation)
            {
                currentAnimation = animation;
                animator.CrossFade(animation, crossFade);
            }
            return getAnimationTime(animation);
        }

        public AnimationClip getAnimationClipByName(string name)
        {
            return animationClipList.Find(a => a.name == name);
        }

        public float getAnimationTime(string animationName)
        {
            AnimationClip animation = getAnimationClipByName(animationName);
            if (animation)
            {
                return animation.length;
            }
            return 0;
        }
    }
}

using sensorX.Models;

namespace sensorX.Services
{
    public class MotionHierarchyValidator
    {
        public bool Validate(MotionNode node)
        {
            // BASE CASE   reached the top of the hierarchy.
            if (node.Parent == null)
            {
                return true;
            }

            // RECURSIVE CASE Move one level higher.
            return Validate(node.Parent);
        }
    }
}
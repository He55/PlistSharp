namespace PlistSharp
{
    public class PlistDate : PlistNode
    {
        public PlistDate(PlistStructure? parent = null)
        {
            CreatePlistNode(plist_type.PLIST_DATE, parent);
        }

        public PlistDate(plist_t node, PlistStructure? parent = null)
        {
            _node = node;
            _parent = parent;
        }

        public PlistDate(timeval t)
        {
            CreatePlistNode(plist_type.PLIST_DATE);
            LibPlist.plist_set_date_val(_node, (int)t.tv_sec, t.tv_usec);
        }

        public override PlistNode Clone()
        {
            PlistDate plistDate = new PlistDate();
            timeval t = GetValue();
            LibPlist.plist_set_date_val(plistDate._node, (int)t.tv_sec, t.tv_usec);

            return plistDate;
        }

        public void SetValue(timeval t)
        {
            LibPlist.plist_set_date_val(_node, (int)t.tv_sec, t.tv_usec);
        }

        public timeval GetValue()
        {
            LibPlist.plist_get_date_val(_node, out int tv_sec, out int tv_usec);
            timeval t = new timeval
            {
                tv_sec = tv_sec,
                tv_usec = tv_usec
            };
            return t;
        }
    }
}

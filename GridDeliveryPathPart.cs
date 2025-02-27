using System.Collections;
using System.Collections.Generic;

/*
Both a single part and a doubly-linked-list path of parts:
* Knows where it is
* Knows it's previous and/or next linked part (double-linked list)
* Knows if it's active
* Understands it's direction and can swap it
* Handles part and source attaching rules
*/
public class GridDeliveryPathAttachablePart
{
    public IGridDeliveryPathPart Part;
    public GridDeliveryPathAttachablePart? PrevPart;
    public GridDeliveryPathAttachablePart? NextPart;
    public bool Active;
    public bool Flipped;
    public IGridDeliveryPathSource? Source;

    public GridDeliveryPathAttachablePart(IGridDeliveryPathPart part)
    {
        Part = part;
        PrevPart = null;
        NextPart = null;
        Active = false;
        Flipped = false;
        Source = null;
    }

    public override string ToString()
    {
        string s = $"[{Part.GridDeliveryPathGroupType}:{Part.GridCell}]{GetVisualType()}";
        if (Source != null) {
            s = $"$->{s}";
        }
        if (PrevPart != null) {
            s = $":{s}";
        }
        if (NextPart != null) {
            s += $"->{NextPart}";
        }
        return s;
    }

    /*
    The part is part of a doubly-linked-list, and
    there may be cases, like when merging, where we
    need to swap the direction of the list.
    direction:
    * 0 = both
    * 1 = previous
    * 2 = next
    */
    public void Swap(int direction = 0)
    {
        // Swap this part
        GridDeliveryPathAttachablePart? nextPart = NextPart;
        NextPart = PrevPart;
        PrevPart = nextPart;
        UpdateVisualType();
        // Swap previous part if there is one
        if (PrevPart != null && direction != 2)
        {
            PrevPart.Swap(1);
        }
        // Swap next part of there is one
        if (NextPart != null && direction != 1)
        {
            NextPart.Swap(2);
        }
    }

    /*
    Used to make sure we're not trying to attach to our
    same path creating a loop
    */
    public bool IsInPath(GridDeliveryPathAttachablePart part, int direction = 0)
    {
        if (part == PrevPart)
        {
            return true;
        }
        if (part == NextPart)
        { 
            return true;
        }
        if (PrevPart != null && direction != 2 && PrevPart.IsInPath(part, 1))
        {
            return true;
        }
        if (NextPart != null && direction != 1 && NextPart.IsInPath(part, 2))
        {
            return true;
        }
        return false;
    }

    /*
    Do a primary to secondary join assuming the
    calling code has already determined which should
    be the primary
    */
    public bool TryAttachPrimarySecondary(GridDeliveryPathAttachablePart primary, GridDeliveryPathAttachablePart secondary)
    {
        // Can we attach it to the tail?
        if (primary.NextPart == null)
        {
            // If the secondary already has a previous, swap it
            if (secondary.PrevPart != null)
            {
                secondary.Swap();
            }
            // If the secondary doesn't have a previous now, join it
            if (secondary.PrevPart == null)
            {
                primary.NextPart = secondary;
                secondary.PrevPart = primary;
                primary.UpdateVisualType();
                secondary.UpdateVisualType();
                // Copy the active and flipped properties
                secondary.SetActive(primary.Active);
                secondary.SetFlipped(primary.Flipped);
                return true;
            }
        }
        // Can we attach it to the head if it's not
        // attached to a source?
        else if (primary.Source == null)
        {
            // If the secondary already has a next, swap it
            if (secondary.NextPart != null)
            {
                secondary.Swap();
            }
            // If the secondary doesn't have a next now, join it
            if (secondary.NextPart == null)
            {
                // Just join the part
                primary.PrevPart = secondary;
                secondary.NextPart = primary;
                primary.UpdateVisualType();
                secondary.UpdateVisualType();
                // Copy the active and flipped properties
                secondary.SetActive(primary.Active);
                secondary.SetFlipped(primary.Flipped);
                return true;
            }
        }
        return false;
    }

    /*
    Try to attach a new part to this part dealing
    with active and order
    */
    public bool TryAttach(GridDeliveryPathAttachablePart part)
    {
        // If this part isn't in the same group, we can't join them
        if (part.Part.GridDeliveryPathGroupType != Part.GridDeliveryPathGroupType) {
            return false;
        }

        // If this part is already linked on both ends,
        // we can't join with any part
        if (PrevPart != null && NextPart != null)
        {
            return false;
        }

        // If the new part is already linked on both ends,
        // we can't join with this part
        if (part.PrevPart != null && part.NextPart != null)
        {
            return false;
        }

        // If this piece is already attached to this same path,
        // bail out, so we don't create loops
        if (IsInPath(part))
        {
            return false;
        }

        // If the current and new part are both active,
        // they can't attach because we can't merge or
        // flip directions
        if (Active && part.Active)
        {
            return false;
        }

        // If the current part is active or the new part
        // doesn't have a previous link, preserve this part's
        // path order
        if (Active || (part.PrevPart == null && part.Source == null))
        {
            bool attached = TryAttachPrimarySecondary(this, part);
            if (attached)
            {
                return true;
            }
        }
        // Preserve the new part's order
        else
        {
            bool attached = TryAttachPrimarySecondary(part, this);
            if (attached)
            { 
                return true;
            }
        }
        return false;
    }

    /*
    Detach this part from its path possibly splitting
    the path in two
    */
    public void Detach()
    {
        DetachSource();
        // Detach from previous part
        if (PrevPart != null)
        {
            PrevPart.NextPart = null;
            PrevPart.UpdateVisualType();
            PrevPart = null;
        }
        // Detach from next part
        if (NextPart != null)
        {
            NextPart.PrevPart = null;
            NextPart.UpdateVisualType();
            // For the next part, reset the active and flipped properties
            NextPart.SetActive(false);
            NextPart.SetFlipped(false);
            NextPart = null;
        }
        UpdateVisualType();
    }

    /*
    Activating a part will need to activate the parts
    joined with it.
    direction:
    * 0 = both
    * 1 = previous
    * 2 = next
    */
    public void SetActive(bool active, int direction = 0)
    {
        Active = active;
        Part.GridDeliveryPathPartUpdateActive(active);
        // Changed active status for previous if there is one
        if (PrevPart != null && direction != 2)
        {
            PrevPart.SetActive(active, 1);
        }
        // Changed active status for next if there is one
        if (NextPart != null && direction != 1)
        {
            NextPart.SetActive(active, 2);
        }
    }

    /*
    Try to attach this part to a source activating
    this part and its path.  This may also result in
    swapping the direction to make the source the head.
    */
    public bool TryAttachSource(IGridDeliveryPathSource source)
    {
        // If this part isn't attachable to this source group, bail out
        if (!Part.AttachableGridDelivaryPathSourceGroupTypes.Contains(source.GridDeliveryPathSourceGroupType)) {
            return false;
        }

        // If this part is already linked on both ends,
        // we can't attach it to a source
        if (PrevPart != null && NextPart != null)
        {
            return false;
        }

        // If this source is already attached to a part, bail out
        if (source.GridDeliveryPathPart != null)
        {
            return false;
        }

        // If this part isn't already directly attached to a source,
        // and it's not active, we can try and attach to the source
        if (Source == null && !Active)
        {
            if (PrevPart != null)
            {
                Swap();
            }
            if (PrevPart == null)
            {
                Source = source;
                Source.GridDeliveryPathSourceUpdateAttached(true);
                Source.GridDeliveryPathPart = this;
                UpdateVisualType();
                SetActive(true);
                return true;
            }
        }
        return false;
    }

    // Detach the part's source if present
    public void DetachSource()
    {
        if (Source != null)
        {
            Source.GridDeliveryPathPart = null;
            Source.GridDeliveryPathSourceUpdateAttached(false);
            Source = null;
            UpdateVisualType();
            // Once detached clear the active and flipped properties
            SetActive(false);
            SetFlipped(false);
        }
    }

    /*
    For active parts/paths, allow the effective/temporary
    direction to be flipped.
    direction:
    * 0 = both
    * 1 = previous
    * 2 = next
    */
    public void SetFlipped(bool flipped, int direction = 0)
    {
        Flipped = flipped;
        Part.GridDeliveryPathPartUpdateFlipped(flipped);
        // Changed flipped status for previous if there is one
        if (PrevPart != null && direction != 2)
        {
            PrevPart.SetFlipped(flipped, 1);
        }
        // Changed flipped status for next if there is one
        if (NextPart != null && direction != 1)
        {
            NextPart.SetFlipped(flipped, 2);
        }
    }

    // Get the head of the path for this part
    public GridDeliveryPathAttachablePart GetHead()
    {
        if (PrevPart != null)
        {
            return PrevPart.GetHead();
        }
        return this;
    }

    /*
    Determine the part type/shape based on where it is
    in the path's shape.  This type string can be used
    to decide what to render.
    */
    public string GetVisualType()
    {
        if (Part.GridCell == null) {
            throw new Exception("Part GridCell is not set");
        }

        // The type ID is two characters representing
        // where the previous part or source is and where
        // the next part is
        string prevDir = "-";
        string nextDir = "-";
        // Look at the previous part
        if (Source != null)
        { 
            if (Source.GridCell == null) {
                throw new Exception("Source GridCell is not set");
            }
            if (Source.GridCell.y < Part.GridCell.y)
            {
                prevDir = "U";
            }
            else if (Source.GridCell.y > Part.GridCell.y)
            {
                prevDir = "D";
            }
            else if (Source.GridCell.x < Part.GridCell.x)
            {
                prevDir = "L";
            }
            else
            {
                prevDir = "R";
            }
        }
        // Look at the source
        if (PrevPart != null)
        {
            if (PrevPart.Part == null) {
                throw new Exception("PrevPart Part is not set");
            }
            if (PrevPart.Part.GridCell == null) {
                throw new Exception("PrevPart Part GridCell is not set");
            }
            if (PrevPart.Part.GridCell.y < Part.GridCell.y)
            {
                prevDir = "U";
            }
            else if (PrevPart.Part.GridCell.y > Part.GridCell.y)
            {
                prevDir = "D";
            }
            else if (PrevPart.Part.GridCell.x < Part.GridCell.x)
            {
                prevDir = "L";
            }
            else
            {
                prevDir = "R";
            }
        }
        // Look at the next part
        if (NextPart != null)
        {
            if (NextPart.Part == null) {
                throw new Exception("NextPart Part is not set");
            }
            if (NextPart.Part.GridCell == null) {
                throw new Exception("NextPart Part GridCell is not set");
            }
            if (NextPart.Part.GridCell.y < Part.GridCell.y)
            {
                nextDir = "U";
            }
            else if (NextPart.Part.GridCell.y > Part.GridCell.y)
            {
                nextDir = "D";
            }
            else if (NextPart.Part.GridCell.x < Part.GridCell.x)
            {
                nextDir = "L";
            }
            else
            {
                nextDir = "R";
            }
        }
        return $"{prevDir}{nextDir}";
    }

    // Update visual type in wrapped part
    public void UpdateVisualType() {
        Part.GridDeliveryPathPartUpdateVisualType(GetVisualType());
    }
}

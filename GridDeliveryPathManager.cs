using System.Collections;
using System.Collections.Generic;
// UNITY: Uncomment this line to use it as a Unity component script
// using UnityEngine;

/*
Basic idea:
-----------
The basic idea is that we need to be able to place path parts
on a grid, and when a part is placed, it needs to automatically
join with existing parts creating a paths.  To do this we need
to be able to do this we need to be able to:
1. Have a container that keeps track of all parts on the grid
2. When a part is added to this container...
    2.a. Find what is above, below, left, and right of that new part
    2.b. For each surrounding part, ask:
        | "Hey buddy, what are you connected to already?"
        Based on the above, we can start to decide if and how to
        connect to the surrounding part.

Hey buddy!:
-----------
So how does a part know what it's connected to?  Lets look at
data-structures or different container types we can use to.

    List/Array of parts:
    ---------------------
    Lets say we are just using a basic list of parts for our path:
    
    [ A ][ B ][ C ][ D ]
    
    The list itself knows what it contains, but if you are placing a
    part that is below B in the grid, does B know that it's already
    connected to A & C? No.  B is blind to what it is connected.  You
    could use the list instead and loop forward or backward through
    the list to find the connected parts, but lets see if there is
    a cleaner option where B knows more.
    
    Linked-list:
    ------------
    A linked-list is a data-structure where the elements of the list
    know the next thing in the list (if any) that they are connected
    to.  This allows you to have algorithms that easily work within
    an item in the list and then just jump to the next one.  It also
    allows you to add an item to the head or tail of the list without
    creating a new list or shifting all of the elements over.  It's
    very likely that the basic list above is using a linked-list under
    the hood to make changes fast, but we don't get to see that part
    or take advantage of it.
    
    A link list starts by adding a "next" property to your items that
    obviously by the name, points at the next item in the list.  They
    visually look like this:
    
    [ A ]->[ B ]->[ C ]->[ D ]

    The "->" arrow represents that "next" property.  To add an item to
    the head/start of a list you simply set the "next" of the new item
    to the head/first item in the list:
    
    new_item.next = head_item
    
    Then you can treat the new item as the head/start of your list. To
    add an item to the tail/end of the list, you simply set the "next"
    of the tail item to the new item:
    
    tail_item.next = new_item
    
    Not if you start at the head item in your list you can follow each
    "next" and get all the way to the end with the item you just added.
    
    So now if you ask the question "Hey B, what are you connected to?",
    B can look at self.next and say "I'm connected to C!".  But what
    about A?  If B only tells us it's connected to C then we may think
    that we can link to it when we actually can't because B is already
    connected on both ends.  Maybe there is a similar data-structure
    that can look both directions.
    
    Doubly-linked-list:
    -------------------
    What happens if you take a linked-list that has a "next" pointing
    at the next item in the list, and also add a "prev" that is pointing
    at the previous item in the list?  This is called a doubly-linked-list.
    It has all the benefits of a (singularly) linked-list, but adds the
    benefit that from an item you can look and walk both directions.  They
    visually look like this:
    
    [ A ]<->[ B ]<->[ C ]<->[ D ]
    
    where the left of the 2-way arrow is "prev" and the "next" is the right
    side.  Adding an item to a doubly-linked-list is a little more complicated
    because you need to maintain 2 links.  If you want to add an item to the
    head/start you need to set:
    
    new_item.next = head_item
    head_item.prev = new_item

    If you want to add an item to the tail/end you need to do roughly the opposite:
    
    tail_item.next = new_item
    new_item.prev = tail_item
    
    So now if you ask the question "Hey B, what are you connected to?",
    B can look at self.prev and self.next and say "I'm connected to A & C!".
    Perfect!  Now we know we can't connect a new part to B.  What about A?
    A has a self.next pointing at B, but it doesn't have a self.prev, so we
    can connect a new part as the previous.  What about D?  D has a self.prev
    but not a self.next so we can connect as the next part.
    
    Doubly-linked-lists also give us an easy way to split a list into two.
    Lets say instead of adding a part to a path we are deleting a part in the
    middle:
    
    [ A ]<->[ B ]<->[ C ]<->[ D ]<->[ E ]
    
    If we want to delete C, we need to set:
    
    B.next = None
    D.prev = None
    
    Pretty simple and since the items in the list are still keeping track of
    their "prev" and "next", we automatically get 2 lists:
    
    [ A ]<->[ B ]
    [ D ]<->[ E ]
    
    Lets say now you want to add C back in and join the lists back together.
    For that you would need to set:
    
    B.next = C
    C.prev = B
    C.next = D
    D.prev = C
    
    Now you are back to a single list:
    
    [ A ]<->[ B ]<->[ C ]<->[ D ]<->[ E ]
    
    Looks like a doubly-linked-list is the data structure we want to start with.

Direction & Swapping:
---------------------
Since we're talking about paths, direction is going to have to be something
we understand.  Since we're using a doubly-linked-list, lets use "prev" to
"next" as our direction.  We'll logically think about anything flowing through
this path as going from the "prev" part, to the current part, and then on to
the "next" part.

So what happens when we have 2 paths that if you add one more part would be
joined together?  This is pretty easy if at the point where they would join
you have one path's head already facing the tail of the other path (or the
other way around).  To visualize this We'll add a P and N market to our 2-way
arrow do you can see the direction:

[ A ]<P-N>[ B ] ... [ D ]<P-N>[ E ]

If I add a C at the ... spot it's pretty easy because B is a tail and D is a
head:

B.prev = C
C.next = B
C.prev = D
D.next = C

Now we have:

[ A ]<P-N>[ B ]<P-N>[ C ]<P-N>[ D ]<P-N>[ E ]

and a path that still flows one consistent direction.  It also works the opposite
way:

[ A ]<N-P>[ B ] ... [ D ]<N-P>[ E ]

because we still have a head facing a tail with B as the head and D as the tail:

B.next = C
C.prev = B
C.next = D
D.prev = C

this gives us:

[ A ]<N-P>[ B ]<N-P>[ C ]<N-P>[ D ]<N-P>[ E ]

still with the combined path all going the same direction.  What what about:

[ A ]<N-P>[ B ] ... [ D ]<P-N>[ E ]

In this case we have a head facing a head and 2 paths flowing different
directions.  How do we join them with a C?  Do do this we need to swap one
of the paths.  For each part in the path we need to swap the "prev" and
"next" to change the direction so wen we add the C the full path will be
flowing the same direction.  To swap you need set:

old_next = self.next
self.next = self.prev
self.prev = old_next

on each part.  So lets swap the D-E path:

[ A ]<N-P>[ B ] ... [ D ]<N-P>[ E ]

Now we can add C in and have a good, single-direction path:

[ A ]<N-P>[ B ]<N-P>[ C ]<N-P>[ D ]<N-P>[ E ]

Great, but how do you decide which path to swap?  If at the time you add C,
you don't really care what direction the 2 paths or the new combined paths
are flowing, then it doesn't really matter which one you swap.  Just
pick a pattern and be consistent.

Sources & Active Paths:
-----------------------
Remember not caring what direction your paths are flowing or which one
to swap?  There is a point where you will care.  If you have your path
connected to a source, you need to care about your path's direction because
it needs to flow away from the source.  We'll call this an "active" path.
If a path is active, you can't change it's direction.

We will also keep track of what part is connected to the "source".  The
source is like a special "prev" link, but we're pointing at a source instead
of another, previous part.  Remember we flow from previous to next, so
the source is logically the "prev" for the part that is connected to it.

This now changes your rules for adding to a path or joining paths together.
So lets start with our basic rules.  We refer to our 2 parts as E (existing)
and N (new)

1. If E.next and E.prev are already set, you can't join the parts.
2. If N.next and N.prev are already set, you can't join the parts.  This is
   mostly an obvious one, you can't join a part that is already in the
   middle of a path, but it's one you need to cover as you are iterating
   over all parts deciding what can be joined and what can't.  You aren't
   always dealing with a new, single part.
3. If E.next and N.next are both set or E.prev and N.prev are both set, then
   you have a head facing a head or tail facing a tail and you need to swap
   one of the paths before joining.
4. If E.prev is set, attach N as E.next or if E.next is set, attach N as E.prev.

Now for the Source rules:

5. If E.source is set, this is the same as E.prev being set.  You can only attach
   N if E.next is not set.
6. If N.source is set, this is the same as N.prev being set,  You can only attach
   E if N.next is not set.

So what about Active rules?  These modify the rules around swapping:

7. (rule 3 updated) If you have a head facing a head or tail facing a tail,
   if E.active, you have to swap N.  If N.active, you have to swap E.
8. (rule 3 update) If E.active and N.active, you can't attach at all.  The only
   way you could support attaching 2 active paths is if you were building a
   "tree" of paths with multiple sources, but that's not what we're building.
   Paths only have a single source, but one track could end and empty on to another.

How do paths get activated?  A path is activated when a Source is attached to one
of it's ends.  This has a couple rules of its own.  We'll call the end E:

1. If E.prev and E.next, the source can't be attached.
2. If E.active, the source can't be attached to a path that is already active.
3. If E.prev, the path needs to be swapped so E.prev is clear, and the path is
   flowing away from the source.  Remember the source logically is the path part's
   "prev".

Temporary Direction Flipping:
-----------------------------
You may want to temporarily flip the direction a path flows to flow "things" back
to the source.  For this you wouldn't want to actually swap the parts and change
the direction of the path and how it's connected to the source.  That would make a
mess of your rules.  Instead you can have a separate "flipped" flag on each part
that will tell you the effective direction for that part vs how it's actually
structured so you can easily flip it back.

Implementation:
---------------
For for implementation we're going to break down the problem into 2 classes:
1. DeliveryPathManager - Responsible for keeping track of all parts across all
   paths, and dealing with new parts being added to the "grid" and triggering
   joining with parts around it.
2. DeliveryPathPart - This is both our part as well as the doubly-linked-list
   of parts that make up a path.  It is responsible for all of the part and source
   attaching rules.
*/

/*
Management class that understands all of parts across all paths.
* Supports adding a new part
* When a new part is added, finds the existing, surrounding parts
  and tries to make attachments
* Supports removing a part and breaking paths
* Supports adding a new source
* When a new source is added, finds the existing, surrounding parts
  and tries to attach to a single path
* Supports removing sources and deleting path parts
*/
// UNITY: Add the : MonoBehaviour back in to use it as a Unity component script
public class GridDeliveryPathManager // : MonoBehaviour
{
    public List<GridDeliveryPathAttachablePart> Parts;
    public List<IGridDeliveryPathSource> Sources;

    public GridDeliveryPathManager()
    {
        Parts = new List<GridDeliveryPathAttachablePart>();
        Sources = new List<IGridDeliveryPathSource>();
    }

    // Utility to find an existing path part at a potition
    // in the grid
    public GridDeliveryPathAttachablePart? FindPartAt(Vector2Int cell)
    {
        foreach (GridDeliveryPathAttachablePart part in Parts)
        {
            if (part.Part == null) {
                throw new Exception("Part is not set");
            }
            if (part.Part.GridCell == null) {
                throw new Exception("Part GridCell is not set");
            }
            if (part.Part.GridCell.x == cell.x && part.Part.GridCell.y == cell.y)
            {
                return part;
            }
        }
        return null;
    }

    // Utility to find an existing source at a position
    // in the grid
    public IGridDeliveryPathSource? FindSourceAt(Vector2Int cell)
    {
        foreach (IGridDeliveryPathSource source in Sources)
        {
            if (source.GridCell == null) {
                throw new Exception("Source GridCell is not set");
            }
            if (source.GridCell.x == cell.x && source.GridCell.y == cell.y)
            {
                return source;
            }
        }
        return null;
    }

    // Is the cell taken by an existing source or path part?
    public bool IsCellTaken(Vector2Int cell)
    {
        return FindPartAt(cell) != null || FindSourceAt(cell) != null;
    }

    // Add a part to the manager at a location
    public bool AddPart(IGridDeliveryPathPart part)
    {
        if (part.GridCell == null) {
            throw new Exception("Part GridCell is not set");
        }

        // If Cell is already taken, bail out
        if (IsCellTaken(part.GridCell)) {
            return false;
        }

        // Wrap and add the part
        GridDeliveryPathAttachablePart newPart = new GridDeliveryPathAttachablePart(part);
        Parts.Add(newPart);

        // Look at surrounding cells
        List<Vector2Int> surrounding = new List<Vector2Int> {
            // Up
            new Vector2Int(part.GridCell.x, part.GridCell.y - 1),
            // Down
            new Vector2Int(part.GridCell.x, part.GridCell.y + 1),
            // Left
            new Vector2Int(part.GridCell.x - 1, part.GridCell.y),
            // Right
            new Vector2Int(part.GridCell.x + 1, part.GridCell.y)
        };

        // Check for existing, surrounding parts and try to attach them
        foreach (Vector2Int cell in surrounding)
        {
            GridDeliveryPathAttachablePart? surroundingPart = FindPartAt(cell);
            if (surroundingPart != null) {
                newPart.TryAttach(surroundingPart);
            }
        }

        // Attach to surrounding sources to try and attach to
        foreach (Vector2Int cell in surrounding)
        {
            IGridDeliveryPathSource? surroundingSource = FindSourceAt(cell);
            if (surroundingSource != null) {
                newPart.TryAttachSource(surroundingSource);
            }
        }

        return true;
    }

    // Remove a part from the manager at a location
    public void RemovePart(Vector2Int gridCell)
    {
        GridDeliveryPathAttachablePart? part = FindPartAt(gridCell);
        if (part != null)
        {
            part.Detach();
            Parts.Remove(part);
        }
    }

    // Add a source to the manager at a location
    public bool AddSource(IGridDeliveryPathSource source)
    {
        if (source.GridCell == null) {
            throw new Exception("Source GridCell is not set");
        }

        //  If Cell is already taken, bail out
        if (IsCellTaken(source.GridCell))
        {
            return false;
        }

        // Add the source
        Sources.Add(source);

        // Look at surrounding cells
        List<Vector2Int> surrounding = new List<Vector2Int> {
            // Up
            new Vector2Int(source.GridCell.x, source.GridCell.y - 1),
            // Down
            new Vector2Int(source.GridCell.x, source.GridCell.y + 1),
            // Left
            new Vector2Int(source.GridCell.x - 1, source.GridCell.y),
            // Right
            new Vector2Int(source.GridCell.x + 1, source.GridCell.y)
        };

        // Check for existing, surrounding parts and try to attach them
        foreach (Vector2Int cell in surrounding)
        {
            GridDeliveryPathAttachablePart? surroundingPart = FindPartAt(cell);
            if (surroundingPart != null) {
                surroundingPart.TryAttachSource(source);
            }
        }

        return true;
    }

    // Remove a source from the manager at a location
    public void RemoveSource(Vector2Int gridCell)
    {
        IGridDeliveryPathSource? source = FindSourceAt(gridCell);
        if (source != null)
        {
            if (source.GridDeliveryPathPart != null)
            {
                source.GridDeliveryPathPart.DetachSource();
            }
            Sources.Remove(source);
        }
    }

    // Go through all parts and reduce them down to all the
    // distinct paths.
    public List<GridDeliveryPathAttachablePart> GetAllPaths()
    {
        HashSet<GridDeliveryPathAttachablePart> paths = new HashSet<GridDeliveryPathAttachablePart>();
        foreach (GridDeliveryPathAttachablePart part in Parts)
        {
            paths.Add(part.GetHead());
        }
        return paths.ToList();
    }

    // Get a list of all sources
    public List<IGridDeliveryPathSource> GetAllSources()
    {
        return Sources;
    }
}

/*
Interface for a delivery path part.  This part will be wrapped
by a GridDeliveryPathAttachablePart to give the path logic.

The interface needs to provide the path part group to limit what
path part types can be attached to.  For example you can't attach
a pipe to a conveyor-belt.

The interface needs to also provide the source type groups the
path can attach to.  For example a conveyor can attach to a mine or
a smelting fectory, but can't connect to a fule pump because that
rewuires a pipe.
*/
public interface IGridDeliveryPathPart : IGridPositioned
{
    int GridDeliveryPathGroupType { get; }
    HashSet<int> AttachableGridDelivaryPathSourceGroupTypes { get; }

    void GridDeliveryPathPartUpdateVisualType(string visualType);
    void GridDeliveryPathPartUpdateActive(bool active);
    void GridDeliveryPathPartUpdateFlipped(bool flipped);
}

/*
Interface for delivery sources.

The interface needs to define what source group this source is in
that matches up with the path part attachable source group types.

The interface also needs to provide a property for the path part
the source may be attached to.  The path part uses and sets the
path property on the source and the source itself can use this
property to know what it's attached to like pushing an item onto
the path.
*/
public interface IGridDeliveryPathSource : IGridPositioned
{
    int GridDeliveryPathSourceGroupType { get; }
    GridDeliveryPathAttachablePart? GridDeliveryPathPart { get; set; }

    void GridDeliveryPathSourceUpdateAttached(bool attached);
}

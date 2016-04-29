using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * BASED ON:
 * 
 * 
 * Rectangle Packer v1.2.1
 *
 * Copyright 2012 Ville Koskela. All rights reserved.
 *
 * Email: ville@villekoskela.org
 * Blog: http://villekoskela.org
 * Twitter: @villekoskelaorg
 *
 * You may redistribute, use and/or modify this source code freely
 * but this copyright statement must not be removed from the source files.
 *
 * The package structure of the source code must remain unchanged.
 * Mentioning the author in the binary distributions is highly appreciated.
 *
 * If you use this utility it would be nice to hear about it so feel free to drop
 * an email.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. *
 *
 */


public class PpRectPacker {

	public static string VERSION = "1.2.1";
    private int mWidth = 0;
    private int mHeight = 0;

    private List<MpRectSortableSize> mInsertList = new List<MpRectSortableSize>();

    private List<PpIntegerRectangle> mInsertedRectangles = new List<PpIntegerRectangle>();
    private List<PpIntegerRectangle> mFreeAreas 		= new List<PpIntegerRectangle>();
    private List<PpIntegerRectangle> mNewFreeAreas 	= new List<PpIntegerRectangle>();

    private PpIntegerRectangle mOutsideRectangle;

    private List<MpRectSortableSize> mSortableSizeStack = new List<MpRectSortableSize>();
    private List<PpIntegerRectangle> mRectangleStack = new List<PpIntegerRectangle>();

	public int rectangleCount{ 
		get{
			return mInsertedRectangles.Count;
		}
	}


    public PpRectPacker(int width, int height)
    {
        reset(width, height);
    }
	
	
    
    
    public void reset(int width, int height){
		
        mOutsideRectangle = new PpIntegerRectangle(width + 1, height + 1, 0, 0);
		
		
		foreach(PpIntegerRectangle iR in mInsertedRectangles){
			freeRectangle( iR);
		}
		mInsertedRectangles = new List<PpIntegerRectangle>();
		
		
		foreach(PpIntegerRectangle iR in mFreeAreas){
			freeRectangle( iR );
		}
		mFreeAreas = new List<PpIntegerRectangle>();

        mWidth = width;
        mHeight = height;
		mFreeAreas = new List<PpIntegerRectangle>();
        mFreeAreas.Add( allocateRectangle(0, 0, mWidth, mHeight) );
		
		foreach(MpRectSortableSize sS in mInsertList){
			freeSize( sS );
		}
		mInsertList = new List<MpRectSortableSize>();
		
		
    }
	

	 public PpIntegerRectangle getRectangle(int index){
		return mInsertedRectangles[index];
	}
	
    public Rect getRectangle(int index, Rect rectangle){
        PpIntegerRectangle inserted = mInsertedRectangles[index];
        rectangle.x = inserted.x;
        rectangle.y = inserted.y;
        rectangle.width = inserted.width;
        rectangle.height = inserted.height;
        return rectangle;
    }


    public int getRectangleId(int index)
    {
        PpIntegerRectangle inserted = mInsertedRectangles[index];
        return inserted.id;
    }


    public void insertRectangle(int width, int height, int id)
    {
        MpRectSortableSize sortableSize = allocateSize(width, height, id);
        mInsertList.Add(sortableSize);
    }


	public int packRectangles(){
		return packRectangles(true);
	}
    public int packRectangles(bool sort)
    {
        if (sort)
        {

			mInsertList.Sort((x, y) => x.width.CompareTo(y.width));
        }

        while (mInsertList.Count > 0)
        {

			MpRectSortableSize sortableSize = PpToolsList.Pop<MpRectSortableSize>( mInsertList ) as MpRectSortableSize;
			
			
			
            int width = sortableSize.width;
            int height = sortableSize.height;

            int index = getFreeAreaIndex(width, height);
            if (index >= 0)
            {
                PpIntegerRectangle freeArea = mFreeAreas[index];
                PpIntegerRectangle target = allocateRectangle(freeArea.x, freeArea.y, width, height);
                target.id = sortableSize.id;

                // Generate the new free areas, these are parts of the old ones intersected or touched by the target
                generateNewFreeAreas(target, mFreeAreas, mNewFreeAreas);
				
					
				foreach(PpIntegerRectangle iR in mNewFreeAreas){
					mFreeAreas.Add( iR );
				}	
				mNewFreeAreas = new List<PpIntegerRectangle>();
				
				mInsertedRectangles.Add( target );
				
            }

            freeSize(sortableSize);
        }

        return rectangleCount;
    }


    private void filterSelfSubAreas( List<PpIntegerRectangle> areas)
    {
		
        for (int i  = areas.Count - 1; i >= 0; i--)
        {
            PpIntegerRectangle filtered = areas[i];
            for (int j = areas.Count - 1; j >= 0; j--)
            {
                if (i != j)
                {
                    PpIntegerRectangle area = areas[j];
                    if (filtered.x >= area.x && filtered.y >= area.y &&
                        filtered.right <= area.right && filtered.bottom <= area.bottom)
                    {
                        freeRectangle(filtered);
						PpIntegerRectangle topOfStack = PpToolsList.Pop<PpIntegerRectangle>(areas);
                        if (i <areas.Count)
                        {
                            // Move the one on the top to the freed position
                            areas[i] = topOfStack;
                        }
                        break;
                    }
                }
            }
        }
    }


    private void generateNewFreeAreas(PpIntegerRectangle target, List<PpIntegerRectangle> areas, List<PpIntegerRectangle> results)
    {
        // Increase dimensions by one to get the areas on right / bottom this rectangle touches
        int x = target.x;
        int y = target.y;
        int right = target.right + 1;
        int bottom = target.bottom + 1;

        for (int i = areas.Count - 1; i >= 0; i--)
        {
            PpIntegerRectangle area = areas[i];
            if (!(x >= area.right || right <= area.x || y >= area.bottom || bottom <= area.y))
            {
                generateDividedAreas(target, area, results);
				PpIntegerRectangle topOfStack = PpToolsList.Pop<PpIntegerRectangle>( areas );
                if (i <areas.Count)
                {
                    // Move the one on the top to the freed position
                    areas[i] = topOfStack;
                }
            }
        }

        filterSelfSubAreas(results);
    }


    private void generateDividedAreas(PpIntegerRectangle divider, PpIntegerRectangle area, List<PpIntegerRectangle> results)
    {
        int count = 0;
        int rightDelta = area.right - divider.right;
        if (rightDelta > 0)
        {
			results.Add( allocateRectangle(divider.right, area.y, rightDelta, area.height) );
            count++;
        }

        int leftDelta = divider.x - area.x;
        if (leftDelta > 0)
        {
			results.Add( allocateRectangle(area.x, area.y, leftDelta, area.height) );
			
            count++;
        }

        int bottomDelta = area.bottom - divider.bottom;
        if (bottomDelta > 0)
        {
			results.Add( allocateRectangle(area.x, divider.bottom, area.width, bottomDelta) );
            count++;
        }

        int topDelta = divider.y - area.y;
        if (topDelta > 0)
        {
			results.Add( allocateRectangle(area.x, area.y, area.width, topDelta) );
			
            count++;
        }

        if (count == 0 && (divider.width < area.width || divider.height < area.height))
        {
            // Only touching the area, store the area itself
			results.Add( area );
        }
        else
        {
            freeRectangle(area);
        }
    }

	
    private int getFreeAreaIndex(int width, int height)
    {
        PpIntegerRectangle best = mOutsideRectangle;
        int index = -1;

        int count = mFreeAreas.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            PpIntegerRectangle free = mFreeAreas[i];
            if (free.x < best.x && width <= free.width && height <= free.height)
            {
                index = i;
                if ((width == free.width && free.width <= free.height && free.right < mWidth) ||
                    (height == free.height && free.height <= free.width))
                {
                    break;
                }
                best = free;
            }
        }

        return index;
    }

	  
    private PpIntegerRectangle allocateRectangle(int x, int y, int width, int height)
    {
        if (mRectangleStack.Count > 0)
        {
			PpIntegerRectangle rectangle = PpToolsList.Pop<PpIntegerRectangle>( mRectangleStack );
            rectangle.x = x;
            rectangle.y = y;
            rectangle.width = width;
            rectangle.height = height;
            rectangle.right = x + width;
            rectangle.bottom = y + height;

            return rectangle;
        }

        return new PpIntegerRectangle(x, y, width, height);
    }


    private void freeRectangle(PpIntegerRectangle rectangle)
    {
		mRectangleStack.Add( rectangle );
    }


    private MpRectSortableSize allocateSize(int width, int height, int id)
    {
        if (mSortableSizeStack.Count > 0)
        {

			MpRectSortableSize size = PpToolsList.Pop<MpRectSortableSize>(mSortableSizeStack);
			
            size.width = width;
            size.height = height;
            size.id = id;

            return size;
        }

        return new MpRectSortableSize(width, height, id);
    }


    private void freeSize(MpRectSortableSize size)
    {
		mSortableSizeStack.Add( size );
    }
}

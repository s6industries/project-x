import pygame
import os
import json

class UI(pygame.sprite.Sprite):

    def __init__(self, x, y, width, height, name='', _type='other'):
        super().__init__()
        self.width = width
        self.height = height
        self.name = name
        self._type = _type

        self.rect = pygame.Rect(x, y, self.width, self.height)
        self.image = ((self.width, self.height))

        self.listAnimated = []
        self.selectors = []
        self.players = []
        self.selected = 0
        self.frameCount = 0
        if self._type == 'animated':
            self.loadAnimated()
        elif self._type == 'selector':
            self.loadSelector()
        elif self._type == 'selector_player':
            self.loadplayer()
        elif self._type == 'bg_image':
            self.objectImage = self.loadBg()
        else:
            self.objectImage = self.loadImage()

    def draw(self, screen):
        if self._type == 'animated':
            self.animate(screen)
        elif self._type == 'selector':
            screen.blit(self.selectors[self.selected], self.rect)
        elif self._type == 'selector_player':
            screen.blit(self.players[0], self.rect)
        else:
            screen.blit(self.objectImage, self.rect)

        # pygame.draw.rect(screen, (255,255,255), self.rect, 2)

    def loadAnimated(self):
        for img in range(7):
            image = f'characters/animatedObj/{self.name}/frame_{img}.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.width, self.height))
            self.listAnimated.append(image)

    def animate(self, screen):
        if(self.frameCount + 1) >= 63:
            self.frameCount = 0

        screen.blit(self.listAnimated[self.frameCount//9], (self.rect.x, self.rect.y))
        self.frameCount += 1
    
    def loadSelector(self):
        for i in range(2):
            image = f'characters/selectors/{self.name}/s_{i}.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.width, self.height))
            self.selectors.append(image)

    def loadplayer(self):
        image = f'characters/selectors/players/{self.name}.png'
        image = pygame.image.load(image)
        image = pygame.transform.scale(image, (self.width, self.height))
        self.players.append(image)

    def loadImage(self):
        image = f'characters/objects/{self.name}.png'
        image = pygame.image.load(image)
        image = pygame.transform.scale(image, (self.width, self.height))
        return image
    
    def loadBg(self):
        image = f'characters/objects/{self.name}.jpg'
        image = pygame.image.load(image)
        return pygame.transform.scale(image, (self.width, self.height))
    
class GUI:

    def __init__(self, x, y, scale, screen, name, bg='test_bg', sfx=None):
        self.x = x
        self.y = y
        self.scale = scale
        self.screen = screen
        self.name = name
        self.bg = bg

        # font
        self.font = pygame.font.SysFont('consolas', 13)
        self.text = None
        
        self.bg_image = None
        self.selections = []
        self.selections2 = []
        self.loadImages()
        self.loadBg()
        self.selected = 0

        self.sfx = sfx # sound effects
        self.game = None
 
    def draw(self):
        # draw the bg image
        if self.bg_image != None:
            self.screen.blit(self.bg_image, (0,0))

        # draw the gui
        if self.name == 'main_menu' and self.game.play:
            self.screen.blit(self.selections2[self.selected], (self.x, self.y))
        else:
            self.screen.blit(self.selections[self.selected], (self.x, self.y))

    def Select(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_UP]:
            self.sfx[0].play()
            self.selected -= 1

            if self.selected < 0:
                if self.name == 'main_menu' and self.game.play:
                    self.selected = len(self.selections2) -1
                else:
                    self.selected = len(self.selections) -1
        elif keys[pygame.K_DOWN]:
            self.sfx[0].play()
            self.selected += 1

            if self.name == 'main_menu' and self.game.play:
                if self.selected > len(self.selections2) -1:
                    self.selected = 0
            else:
                if self.selected > len(self.selections) -1:
                    self.selected = 0
        
        elif keys[pygame.K_SPACE]:
            self.sfx[1].play()
            return self.selected

    def loadImages(self):

        path = f'characters/GUI/{self.name}'
        for filename in os.listdir(path):
            if filename.endswith('.png'):
                image_path = os.path.join(path, filename)
                try:
                    image = pygame.image.load(image_path)
                    image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
                    self.selections.append(image)
                except pygame.error:
                    print('File not found:', image_path)

        # only for main menu
        if self.name == 'main_menu':
            path = f'characters/GUI/main2_menu'
            for filename in os.listdir(path):
                if filename.endswith('.png'):
                    image_path = os.path.join(path, filename)
                    try:
                        image = pygame.image.load(image_path)
                        image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
                        self.selections2.append(image)
                    except pygame.error:
                        print('File not found:', image_path)

    def loadBg(self):
        if self.bg != None:
            image = f'characters/objects/{self.bg}.jpg'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (700, 700))
            self.bg_image = image

    def drawText(self, x, y):
        if self.text:
            text = self.font.render(self.text.title(), True, (102,255,227))
            self.screen.blit(text, (x, y, text.get_width(), text.get_height()))

class CraftingBook:

    def __init__(self, x, y, width, height, screen):
        self.rect = pygame.Rect(x, y, width, height)
        self.screen = screen
        self.page = 0
        self.loadedDataImages = []
        self.allItemsData = []
        self.data = []
        self.loadData()
        self.loadDataImages()

        self.image = pygame.transform.scale(pygame.image.load(f'characters/GUI/crafting_book/s_0.png'), (self.rect.width, self.rect.height))

        self.grid1 = [(135, 265), (183, 265), (231, 265)]
        self.grid2 = [(365, 265), (413, 265), (461, 265)]

        # sfx
        self.sfx = None
        self.flip_sfx = pygame.mixer.Sound('audio/flip.mp3')
        self.sfx_timer = 0

    def draw(self):
        self.screen.blit(self.image, self.rect)

        self.draw_content()

    def draw_content(self):
        self.screen.blit(self.loadedDataImages[self.page]['result'], (140, 155))
        for i, comItems in enumerate(self.loadedDataImages[self.page]['combination']):
            self.screen.blit(comItems, self.grid1[i])

        if (self.page +1) > len(self.data)-1:
             self.screen.blit(self.loadedDataImages[0]['result'], (370, 155))
             for i, comItems in enumerate(self.loadedDataImages[0]['combination']):
                self.screen.blit(comItems, self.grid2[i])
        else:
            self.screen.blit(self.loadedDataImages[self.page+1]['result'], (370, 155))
            for i, comItems in enumerate(self.loadedDataImages[self.page+1]['combination']):
                self.screen.blit(comItems, self.grid2[i])

    def Actions(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_RIGHT]:
            self.flip_sfx.play()
            self.page += 1
            if self.page > len(self.data) -1:
                self.page = 0
        
        elif keys[pygame.K_LEFT]:
            self.flip_sfx.play()
            self.page -= 1
            if self.page < 0:
                self.page = len(self.loadedDataImages) -1
        
        elif keys[pygame.K_SPACE] or keys[pygame.K_f]:
            self.flip_sfx.play()
            return True

    def loadData(self):
        with open('Data/items.json') as file:
            data = json.load(file)

            self.data = data['combination']
            self.allItemsData = data["Items"]

    def loadDataImages(self):
        data = {}
        comItems = []
        for item in self.data:
            if item['result'] in ['boomerang', 'bomb']:
                image = pygame.image.load(f"characters/weapons/{item['result']}/weapon.png")
            else:
                image = pygame.image.load(f"characters/icons/{item['result']}.png")
            image = pygame.transform.scale(image, (75, 75))
            data['result'] = image
            for comItem in item['combination']:
                cimage = pygame.image.load(f'characters/icons/{comItem}.png')
                cimage = pygame.transform.scale(cimage, (40, 40))
                comItems.append(cimage)
            data['combination'] = comItems
            self.loadedDataImages.append(data)
            data = {}
            comItems = []
                
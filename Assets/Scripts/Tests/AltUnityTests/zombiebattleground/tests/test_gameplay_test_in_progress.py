import unittest

from pages.base import CZBTests
from pages.gameplay_page import Gameplay_Page
from pages.main_menu_page import Main_Menu_Page
import xmlrunner


cards = []
logs = []


class CZBPlayTests(CZBTests):

    def setUp(self):
        super(CZBPlayTests, self).setUp()
        self.skip_tutorials()

    def test_start_match(self):
        Main_Menu_Page(self.altdriver).press_battle_button()
        Gameplay_Page(self.altdriver,self.driver).play_a_match()



    # def test_start_match_versus_ai(self):
    #     global logs
    #     # enter Deck Selection
    #     time.sleep(1)
    #     self.altdriver.wait_for_element('Button_Play').mobile_tap()
    #     time.sleep(1)
    #     self.altdriver.wait_for_element('Button_SoloMode').mobile_tap()
    #
    #     cards = self.get_list_of_card_in_deck()
    #     logs.append(str(datetime.datetime.now()) +
    #                 ' Cards in the deck' + str(cards))
    #
    #     # self.altdriver.wait_for_element('Button_SoloMode').mobile_tap()
    #
    #     # while cards.count!=0:
    #     # Play match until you played all the card in the deck once
    #     logs.append(str(datetime.datetime.now())+' Start new game')
    #     self.altdriver.wait_for_element('Button_Battle').mobile_tap()
    #     self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
    #     # self.mulligan_cards()
    #     self.altdriver.wait_for_element('Button_Keep').mobile_tap()
    #
    #     while self.game_not_ended():
    #         self.your_turn()
    #
    #     # allElement=self.altdriver.get_all_elements(enabled=False)
    #     # for element in allElement:
    #     #     logs.append(element.name)
    #     # break
    #     # self.altdriver.find_element('Button_Continue').mobile_tap()
    #
    # def your_turn(self):
    #     logs.append(str(datetime.datetime.now()) +
    #                 ' Your turn. Stil not played yet ' + str(cards))
    #     self.altdriver.wait_for_element(
    #         'EndTurnButton/_1_btn_endturn/EndTurnGlowEffect', timeout=60)
    #     self.check_player_goo_bottle_to_be_refilled()
    #     time.sleep(3)
    #     # self.choose_what_card_to_play()
    #     self.altdriver.wait_for_element(
    #         'EndTurnButton/_1_btn_endturn').mobile_tap()
    #     logs.append(str(datetime.datetime.now())+' End turn.')
    #     time.sleep(1)
    #     # time.sleep(1)
    #     # self.check_opponent_goo_bottle_to_be_refilled()
    #
    # def get_player_board_monster_count(self):
    #     return int(len(self.altdriver.find_elements('BattleField/PlayerBoard/BoardCreature(Clone)')))
    #
    # def get_player_defence(self):
    #     return int(self.altdriver.wait_for_element('Player/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText').get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro'))
    #
    # def get_opponent_defence(self):
    #     return int(self.altdriver.wait_for_element('Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText').get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro'))
    #
    # def get_player_goo(self):
    #     return int(self.altdriver.wait_for_element('Player/OverlordArea/RegularModel/RegularPosition/Gauge/CZB_3D_Overlord_gauge_LOD0/Text').get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro').split('/')[0])
    #
    # def check_player_goo_bottle_to_be_refilled(self):
    #     number_of_bottles = len(
    #         self.altdriver.find_elements('PlayerManaBar/BottleGoo'))
    #     number_of_goo = len(self.altdriver.find_elements(
    #         'PlayerManaBar/BottleGoo/New Sprite Mask/goo'))
    #     self.assertEqual(number_of_bottles, number_of_goo)
    #
    # def check_opponent_goo_bottle_to_be_refilled(self):
    #     number_of_bottles = len(
    #         self.altdriver.find_elements('OpponentManaBar/BottleGoo'))
    #     number_of_goo = len(self.altdriver.find_elements(
    #         'OpponentManaBar/BottleGoo/New Sprite Mask/goo'))
    #     self.assertEqual(number_of_bottles, number_of_goo)
    #
    # def game_not_ended(self):
    #     timeout = 40
    #     print('CheckGameNotEnded')
    #     while True:
    #         playerHealth = int(self.get_player_defence())
    #         if playerHealth <= 0:
    #             logs.append(str(datetime.datetime.now())+" End game ")
    #             return False
    #
    #         try:
    #             self.altdriver.find_element(
    #                 'EndTurnButton/_1_btn_endturn/EndTurnGlowEffect')
    #             return True
    #         except Exception:
    #             print('EndTurnButton not found')
    #         time.sleep(3)
    #         timeout = timeout-3
    #         if(timeout <= 0):
    #             return False
    #
    # def choose_what_card_to_play(self):
    #     global cards
    #     if self.get_player_board_monster_count == 6:
    #         logs.append(datetime.datetime.now(
    #         )+' Player has 6 zombies on the battlefield there is no more room to play a zombie.')
    #         return
    #     print(str(self.get_player_board_monster_count())+' monsters on board')
    #     handCards = []
    #     handCards = self.altdriver.find_elements(
    #         "CreatureCard(Clone)", 'MainCamera')
    #     itemCards = self.altdriver.find_elements(
    #         "ItemCard(Clone)", "MainCamera")
    #     handCards = handCards+itemCards
    #
    #     cardsName = []
    #     for card in handCards:
    #         cardsName.append(self.altdriver.wait_for_element(
    #             'id('+str(card.id)+')/TitleText').get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro'))
    #     logs.append(str(datetime.datetime.now()) +
    #                 ' Cards in the hand' + str(cardsName))
    #
    #     totalGoo = self.get_player_goo()
    #     while True:
    #         ok = 0
    #         playableCards = self.get_playable_cards(handCards)
    #         if(len(playableCards) == 0):
    #             break  # There is no playable card
    #         for card in playableCards:
    #             cardName = self.altdriver.wait_for_element(
    #                 'id('+str(card.id)+')/TitleText').get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro')
    #             if self.is_card_in_the_list(cardName, cards):
    #                 self.play_card(card)
    #                 ok = 1
    #                 break
    #         if ok == 0:
    #             self.play_card(playableCards[0])
    #
    # def check_card_is_playable(self, card, totalGoo):
    #     return self.element_is_enabled(card)
    #
    # def move_card_from_hand_to_battlefield(self, card):
    #     playerBoard = self.altdriver.find_element('PlayerBoard')
    #     gooText = self.altdriver.find_element(
    #         'id('+str(card.id)+')/GooText', 'MainCamera')
    #     self.driver.swipe(int(gooText.x), int(gooText.mobileY), int(
    #         playerBoard.x), int(playerBoard.mobileY), 2000)
    #     time.sleep(3)
    #
    # def get_list_of_card_in_deck(self):
    #     global cards
    #     # Deck selection page
    #     self.altdriver.wait_for_element('Button_Edit').mobile_tap()
    #
    #     # Horde Edititing Page
    #     self.altdriver.wait_for_element('HordeEditingPage(Clone)')
    #     isLastPage=False
    #
    #     while isLastPage==False:
    #
    #         creatureCards = self.altdriver.find_elements(
    #             'Horde/Cards/CreatureCard(Clone)/TitleText', enabled=False)
    #         for card in creatureCards:
    #             cardName=card.get_component_property(
    #                     'TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro')
    #             if cardName in cards:
    #                 isLastPage=True
    #                 break
    #             else:
    #                 cards.append(cardName)
    #         if isLastPage:
    #             break
    #         itemCards = self.altdriver.find_elements(
    #             'Horde/Cards/ItemCard(Clone)/TitleText', enabled=False)
    #         for cardName in itemCards:
    #             cardName=card.get_component_property(
    #                     'TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro')
    #             if cardName in cards:
    #                 isLastPage=True
    #                 break
    #             else:
    #                 cards.append(cardName)
    #         self.altdriver.wait_for_element('Horde/ArrowRightButton').mobile_tap()
    #
    #     self.altdriver.wait_for_element('Button_Save').mobile_tap()
    #     return cards
    #
    # def element_is_enabled(self, card):
    #     cardAvailable = None
    #     try:
    #         cardAvailable = self.altdriver.find_element(
    #             'id('+str(card.id)+')/GlowContainer/Glow')
    #     except:
    #         pass
    #     if cardAvailable == None:
    #         return False
    #     return True
    #
    # def get_playable_cards(self, cards):
    #     playableCards = []
    #     for card in cards:
    #         if(self.element_is_enabled(card)):
    #             playableCards.append(card)
    #     return playableCards
    #
    # def is_card_in_the_list(self, card, cards):
    #     if cards.count(card) == 0:
    #         return False
    #     return True
    #
    # def play_card(self, card):
    #
    #     global cards
    #     cardGoo = self.altdriver.wait_for_element(
    #         'id('+str(card.id)+')/GooText')
    #     cardName = self.altdriver.wait_for_element(
    #         'id('+str(card.id)+')/TitleText').get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro')
    #     cardText = self.altdriver.wait_for_element(
    #         'id('+str(card.id)+')/BodyText').get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro')
    #     board = self.altdriver.wait_for_element('PlayerBoard')
    #
    #     logs.append(str(datetime.datetime.now())+' Play card ' +
    #                 str(cardName) + ' with body '+cardText)
    #     # Swipe card from hand to battlefield
    #     self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
    #                       int(board.x), int(board.mobileY), 2000)
    #     self.select_target_for_spell_or_entry(cardText)
    #     time.sleep(2)
    #     try:
    #         cards.remove(cardName)
    #     except:
    #         pass
    #     print(len(cards))
    #     print(cards)
    #
    # def mulligan_cards(self):
    #     global cards
    #     replacePanel = self.altdriver.wait_for_element('Replace_Panel')
    #     mulliganCards = self.altdriver.find_elements(
    #         'MulliganCard_Unit(Clone)/Text_Title')
    #     mulliganCards = mulliganCards + self.altdriver.find_elements(
    #             'MulliganCard_Spell(Clone)/Text_Title')
    #     for mulliganCard in mulliganCards:
    #         cardName = mulliganCard.get_component_property(
    #             'TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro')
    #         if self.is_card_in_the_list(cardName, cards):
    #             mulliganCard.mobile_dragToElement(replacePanel)
    #     self.altdriver.wait_for_element('Button_Keep').mobile_tap()
    #
    # def select_target_for_spell_or_entry(self, cardText):
    #     global logs
    #     try:
    #         self.altdriver.wait_for_element(
    #             'AttackArrowVFX_Object(Clone)', timeout=2)
    #         logs.append(cardText)
    #         if 'Destroy a Frozen zombie' in cardText:
    #             logs.append("Frozen")
    #             creatures = self.altdriver.find_elements(
    #                 'BoardCreature(Clone)')
    #             for creature in creatures:
    #                 color = self.altdriver.find_element('id('+creature.id+')/Other/Frozen').get_component_property(
    #                     '.UnityEngine.SpriteRender', 'color', 'UnityEngine.CoreModule')
    #
    #                 logs.append(str(color)+" color")  # delete after
    #                 if color == 1:
    #                     creature.mobile_tap()
    #         else:
    #             if 'Destroy a Heavy zombie' in cardText:
    #                 logs.append("Heavy")
    #                 creatures = self.altdriver.find_elements(
    #                     'BoardCreature(Clone)')
    #                 for creature in creatures:
    #                     try:
    #                         self.altdriver.find_element(
    #                             'id('+creature.id+')/Heavy_Arrival_VFX(Clone)')
    #                         creature.mobile_tap()
    #                     except:
    #                         pass
    #             else:
    #                 self.altdriver.find_element('BoardCreature(Clone)').mobile_tap()
    #     except:
    #         pass
    #
    # def tearDown(self):
    #     global logs
    #     for entry in logs:
    #         print(entry)


if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
